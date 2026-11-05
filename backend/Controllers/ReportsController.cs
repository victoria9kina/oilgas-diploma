using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OilGasApi.Data;
using OilGasApi.Models;

namespace OilGasApi.Controllers;

// отчёты только для вошедших пользователей
[ApiController]
[Authorize]//атрибут для отчетов, чтобы только вошедшие пользователи могли их видеть
[Route("api/reports")]//3 отчёта
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ReportsController(AppDbContext db)
    {
        _db = db;
    }

    // добыча и бурение по месяцам в одной таблице
    [HttpGet("monthly-summary")]
    public IActionResult MonthlySummary(DateOnly? from, DateOnly? to)
    {
        var productions = _db.Productions
            .Where(p => (from == null || p.Date >= from) && (to == null || p.Date <= to))
            .ToList();
        var drillings = _db.DrillingActivities.ToList();

        // соединяем добычу и бурение по дате
        var summary = productions
            .Join(drillings,
                production => production.Date, // поле для связи в добыче
                drilling => drilling.Date, // поле для связи в бурении
                (production, drilling) => new // что кладём в одну строку отчёта
                {
                    production.Date,
                    production.OilTotal,
                    production.GasTotal,
                    production.OilUnconventionalSharePercent,
                    drilling.BrentPrice,
                    drilling.PriceCategory,
                    drilling.WellsCompleted,
                    drilling.MetersPerWell
                })
            .OrderBy(row => row.Date)
            .ToList();

        return Ok(summary);
    }

    // итоговый датасет для прогноза добыча бурение рынок и события по месяцам
    [HttpGet("forecast-dataset")]
    public IActionResult ForecastDataset(DateOnly? from, DateOnly? to)
    {
        var productions = _db.Productions
            .Where(p => (from == null || p.Date >= from) && (to == null || p.Date <= to))
            .ToList();
        var drillings = _db.DrillingActivities.ToList();
        var marketFactors = _db.MarketFactors.ToList();
        var marketEvents = _db.MarketEvents.ToList();

        var dataset = productions
            // сначала добыча плюс бурение
            .Join(drillings,
                production => production.Date,
                drilling => drilling.Date,
                (production, drilling) => new { production, drilling })
            // потом к ним рыночные факторы
            .Join(marketFactors,
                row => row.production.Date,
                marketFactor => marketFactor.Date,
                (row, marketFactor) => new
                {
                    row.production.Date,
                    row.production.OilTotal,
                    row.production.GasTotal,
                    row.production.OilUnconventionalSharePercent,
                    row.drilling.BrentPrice,
                    row.drilling.WellsCompleted,
                    row.drilling.MetersDrilled,
                    marketFactor.OilExportUsdMln,
                    marketFactor.GasExportUsdMln,
                    marketFactor.UsdArsRate,
                    marketFactor.WorldOilDemand,
                    // события этого месяца их может быть несколько
                    Events = marketEvents.Where(e => e.Date == row.production.Date).Select(e => e.Title).ToList(),
                    EventsImpact = marketEvents.Where(e => e.Date == row.production.Date).Sum(e => e.Impact)
                })
            .OrderBy(row => row.Date)
            .ToList();

        return Ok(dataset);
    }

    // доля каждого бассейна за один месяц
    [HttpGet("basin-shares")]
    public IActionResult BasinShares(DateOnly date)
    {
        var production = _db.Productions.FirstOrDefault(p => p.Date == date);
        var basinProductions = _db.BasinProductions.Include(b => b.Basin).Where(b => b.Date == date).ToList();
        if (production == null || basinProductions.Count == 0)
            return NotFound(new { message = $"Нет данных за {date}" });

        // сумма всех бассейнов за месяц
        var basinsTotal = basinProductions.Sum(b => b.Value);

        var shares = basinProductions
            .Select(b => new
            {
                b.BasinId,
                BasinName = b.Basin?.Name,
                b.Value,
                SharePercent = b.SharePercentOf(basinsTotal)
            })
            .OrderByDescending(share => share.Value)
            .ToList();

        return Ok(new
        {
            date,
            nationalOilTotal = production.OilTotal,
            basinsTotal = Math.Round(basinsTotal, 2),
            basins = shares
        });
    }

    // рейтинг бассейнов за год с координатами для карты в дипломе
    [HttpGet("top-basins")]
    public IActionResult TopBasins(int year)
    {
        var yearRecords = _db.BasinProductions
            .Where(b => b.Date >= new DateOnly(year, 1, 1) && b.Date <= new DateOnly(year, 12, 31))
            .ToList();
        if (yearRecords.Count == 0)
            return NotFound(new { message = $"Нет данных за {year} год" });

        var yearTotal = yearRecords.Sum(b => b.Value);

        var ranking = _db.Basins.ToList()
            .Select(basin =>
            {
                // сколько нефти дал этот бассейн за год
                var basinTotal = yearRecords.Where(b => b.BasinId == basin.Id).Sum(b => b.Value);
                return new
                {
                    basin.Code,
                    basin.Name,
                    basin.Latitude,
                    basin.Longitude,
                    Total = Math.Round(basinTotal, 2),
                    SharePercent = Percent.Of(basinTotal, yearTotal)
                };
            })
            .OrderByDescending(row => row.Total)
            .ToList();

        return Ok(ranking);
    }
}
