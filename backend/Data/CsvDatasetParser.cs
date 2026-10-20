using System.Globalization;
using OilGasApi.Models;

namespace OilGasApi.Data;

/// <summary>
/// Разбор CSV Argentina Oil and Gas Dataset на три сущности.
/// Одна строка CSV = один месяц -> 1 Production + 5 BasinProduction + 1 DrillingActivity.
/// </summary>
public static class CsvDatasetParser
{
    public record Result(List<Production> Productions, List<BasinProduction> Basins, List<DrillingActivity> Drilling);

    private static readonly string[] DateFormats =
        ["yyyy-MM-dd", "yyyy-MM", "yyyy-MM-dd HH:mm:ss", "dd/MM/yyyy", "MM/dd/yyyy", "yyyy/MM/dd"];

    public static Result Parse(IEnumerable<string> lines)
    {
        var all = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
        if (all.Count == 0) throw new FormatException("CSV пустой");

        // Разделитель: запятая или точка с запятой
        char sep = all[0].Count(c => c == ';') > all[0].Count(c => c == ',') ? ';' : ',';

        var header = Split(all[0], sep).Select(h => h.ToLowerInvariant()).ToArray();
        var col = header.Select((name, i) => (name, i)).ToDictionary(x => x.name, x => x.i);
        if (!col.ContainsKey("fecha")) throw new FormatException("В CSV нет колонки 'fecha'");

        var result = new Result([], [], []);

        foreach (var line in all.Skip(1))
        {
            var cells = Split(line, sep);
            string? Cell(string name) =>
                col.TryGetValue(name, out var i) && i < cells.Length && cells[i].Length > 0 ? cells[i] : null;

            var date = ParseDate(Cell("fecha"));
            if (date is null) continue; // пропускаем битые строки

            var p = new Production
            {
                Date = date.Value,
                OilConventional = Num(Cell("petroleo_convencional")) ?? 0,
                OilShale = Num(Cell("petroleo_shale")) ?? 0,
                OilTight = Num(Cell("petroleo_tight")) ?? 0,
                GasConventional = Num(Cell("gas_convencional")) ?? 0,
                GasShale = Num(Cell("gas_shale")) ?? 0,
                GasTight = Num(Cell("gas_tight")) ?? 0,
            };
            var oilTotal = Num(Cell("petroleo_total"));
            var gasTotal = Num(Cell("gas_total"));
            p.RecalculateTotals();
            if (oilTotal is not null) p.OilTotal = oilTotal.Value; // доверяем итогу из датасета, если он есть
            if (gasTotal is not null) p.GasTotal = gasTotal.Value;
            result.Productions.Add(p);

            // melt: cuenca_* -> (дата, бассейн, значение)
            foreach (var basin in Models.Basins.All)
            {
                var v = Num(Cell(basin.CsvColumn));
                if (v is not null)
                    result.Basins.Add(new BasinProduction { Date = date.Value, Basin = basin.Code, Value = v.Value });
            }

            result.Drilling.Add(new DrillingActivity
            {
                Date = date.Value,
                BrentPrice = Num(Cell("precio_brent")),
                MetersDrilled = Num(Cell("metros_perforados")),
                WellsCompleted = Int(Cell("pozos_terminados")),
                ActiveUnconventionalWells = Int(Cell("pozos_no_conv_activos")),
            });
        }

        return result;
    }

    private static string[] Split(string line, char sep) =>
        line.Split(sep).Select(s => s.Trim().Trim('"').Trim()).ToArray();

    private static DateOnly? ParseDate(string? s)
    {
        if (s is null) return null;
        if (DateTime.TryParseExact(s, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt) ||
            DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            return new DateOnly(dt.Year, dt.Month, 1);
        return null;
    }

    private static double? Num(string? s) =>
        s is not null && double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : null;

    private static int? Int(string? s) => Num(s) is double d ? (int)Math.Round(d) : null;
}
