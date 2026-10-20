using OilGasApi.Models;

namespace OilGasApi.Services;

/// <summary>
/// Сводные отчёты: запросы, объединяющие данные из разных классов (LINQ Join / GroupBy / Where / Select).
/// Работает с уже загруженными коллекциями, поэтому в запросах можно вызывать методы моделей.
/// </summary>
public static class ReportService
{
    // ---------- 1. Добыча + бурение помесячно (Production ⨝ DrillingActivity) ----------
    public record MonthlySummary(
        DateOnly Date, double OilTotal, double GasTotal, double OilUnconventionalSharePercent,
        double? BrentPrice, string PriceCategory, int? WellsCompleted, double? MetersPerWell);

    public static List<MonthlySummary> MonthlySummaries(
        IEnumerable<Production> productions, IEnumerable<DrillingActivity> drilling, DateOnly? from, DateOnly? to) =>
        productions
            .Where(p => (from == null || p.Date >= from) && (to == null || p.Date <= to))
            .Join(drilling, p => p.Date, d => d.Date, (p, d) => new MonthlySummary(
                p.Date, p.OilTotal, p.GasTotal, p.OilUnconventionalSharePercent(),
                d.BrentPrice, d.PriceCategory(), d.WellsCompleted, d.MetersPerWell()))
            .OrderBy(x => x.Date)
            .ToList();

    // ---------- 2. Доли бассейнов за месяц (BasinProduction ⨝ Production) ----------
    public record BasinShare(string Basin, string? Name, double? Latitude, double? Longitude,
        double Value, double SharePercent, bool IsDominant);

    public record BasinSharesReport(DateOnly Date, double BasinsSum, double NationalOilTotal,
        double BasinsToNationalOilRatio, List<BasinShare> Basins);

    public static BasinSharesReport? BasinShares(
        IEnumerable<BasinProduction> basins, IEnumerable<Production> productions, DateOnly date)
    {
        var month = basins.Where(b => b.Date == date).ToList();
        var national = productions.FirstOrDefault(p => p.Date == date);
        if (month.Count == 0 || national is null) return null;

        var sum = month.Sum(b => b.Value);

        var items = month
            .Join(Models.Basins.All, b => b.Basin, info => info.Code, (b, info) => new BasinShare(
                b.Basin, info.Name, info.Latitude, info.Longitude,
                b.Value, b.SharePercentOf(sum), b.IsDominant(sum)))
            .OrderByDescending(x => x.Value)
            .ToList();

        // Отношение суммы бассейнов к национальной нефти: помогает понять, что именно лежит в cuenca_*
        var ratio = national.OilTotal > 0 ? Math.Round(sum / national.OilTotal, 3) : 0;
        return new BasinSharesReport(date, sum, national.OilTotal, ratio, items);
    }

    // ---------- 3. Годовая сводка (GroupBy по году + Join двух групп) ----------
    public record YearlySummary(int Year, int Months, double AvgOil, double AvgGas,
        double AvgOilUnconventionalSharePercent, double? OilGrowthPercent,
        double? AvgBrent, int TotalWellsCompleted, double TotalMetersDrilled);

    public static List<YearlySummary> Yearly(IEnumerable<Production> productions, IEnumerable<DrillingActivity> drilling)
    {
        var prodByYear = productions
            .GroupBy(p => p.Date.Year)
            .Select(g => new
            {
                Year = g.Key,
                Months = g.Count(),
                AvgOil = Math.Round(g.Average(p => p.OilTotal), 2),
                AvgGas = Math.Round(g.Average(p => p.GasTotal), 2),
                AvgShare = Math.Round(g.Average(p => p.OilUnconventionalSharePercent()), 2)
            });

        var drillByYear = drilling
            .GroupBy(d => d.Date.Year)
            .Select(g => new
            {
                Year = g.Key,
                AvgBrent = g.Where(d => d.BrentPrice != null).Select(d => d.BrentPrice!.Value).DefaultIfEmpty().Average(),
                HasBrent = g.Any(d => d.BrentPrice != null),
                Wells = g.Sum(d => d.WellsCompleted ?? 0),
                Meters = g.Sum(d => d.MetersDrilled ?? 0)
            });

        // Left join: год может быть в добыче, но отсутствовать в бурении
        var joined = prodByYear
            .GroupJoin(drillByYear, p => p.Year, d => d.Year, (p, ds) => new { p, d = ds.FirstOrDefault() })
            .OrderBy(x => x.p.Year)
            .ToList();

        return joined
            .Select((x, i) =>
            {
                double? growth = i > 0 && joined[i - 1].p.AvgOil > 0
                    ? Math.Round((x.p.AvgOil - joined[i - 1].p.AvgOil) / joined[i - 1].p.AvgOil * 100, 2)
                    : null;
                return new YearlySummary(x.p.Year, x.p.Months, x.p.AvgOil, x.p.AvgGas, x.p.AvgShare, growth,
                    x.d is { HasBrent: true } ? Math.Round(x.d.AvgBrent, 2) : null,
                    x.d?.Wells ?? 0, x.d?.Meters ?? 0);
            })
            .ToList();
    }

    // ---------- 4. Влияние цены Brent на добычу и бурение ----------
    public record PriceImpact(string PriceCategory, int Months, double AvgOil,
        double AvgOilUnconventionalSharePercent, double AvgWellsCompleted, double? AvgMetersPerWell);

    public static List<PriceImpact> PriceImpactReport(
        IEnumerable<Production> productions, IEnumerable<DrillingActivity> drilling)
    {
        string[] order = ["Low", "Medium", "High", "Unknown"];

        return productions
            .Join(drilling, p => p.Date, d => d.Date, (p, d) => new { p, d })
            .GroupBy(x => x.d.PriceCategory())
            .Select(g =>
            {
                var mpw = g.Select(x => x.d.MetersPerWell()).Where(v => v != null).Select(v => v!.Value).ToList();
                return new PriceImpact(
                    g.Key,
                    g.Count(),
                    Math.Round(g.Average(x => x.p.OilTotal), 2),
                    Math.Round(g.Average(x => x.p.OilUnconventionalSharePercent()), 2),
                    Math.Round(g.Average(x => (double)(x.d.WellsCompleted ?? 0)), 2),
                    mpw.Count > 0 ? Math.Round(mpw.Average(), 1) : null);
            })
            .OrderBy(x => Array.IndexOf(order, x.PriceCategory))
            .ToList();
    }

    // ---------- 5. Рейтинг бассейнов за год (данные для карты) ----------
    public record BasinRanking(int Rank, string Basin, string? Name, double? Latitude, double? Longitude,
        double TotalValue, double SharePercent, double? ChangePercentVsPreviousYear);

    public static List<BasinRanking> TopBasins(IEnumerable<BasinProduction> basins, int year)
    {
        var list = basins.ToList();

        var current = list.Where(b => b.Date.Year == year)
            .GroupBy(b => b.Basin)
            .Select(g => new { Basin = g.Key, Total = g.Sum(b => b.Value) })
            .ToList();

        var previous = list.Where(b => b.Date.Year == year - 1)
            .GroupBy(b => b.Basin)
            .ToDictionary(g => g.Key, g => g.Sum(b => b.Value));

        var grandTotal = current.Sum(x => x.Total);

        return current
            .OrderByDescending(x => x.Total)
            .Select((x, i) =>
            {
                var info = Models.Basins.Find(x.Basin);
                double? change = previous.TryGetValue(x.Basin, out var prev) && prev > 0
                    ? Math.Round((x.Total - prev) / prev * 100, 2)
                    : null;
                return new BasinRanking(i + 1, x.Basin, info?.Name, info?.Latitude, info?.Longitude,
                    Math.Round(x.Total, 2),
                    grandTotal > 0 ? Math.Round(x.Total / grandTotal * 100, 2) : 0,
                    change);
            })
            .ToList();
    }
}
