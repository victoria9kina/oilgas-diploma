using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OilGasApi.Data;
using OilGasApi.Models;

namespace OilGasApi.Controllers;

/// <summary>
/// Сводные отчёты: объединяют данные нескольких классов.
/// [Authorize] на весь контроллер: без токена ответ 401, с токеном любой роли отчёт доступен.
/// Отчёты используют вычисляемые свойства моделей (PriceCategory, MetersPerWell и др.),
/// SQL о них не знает, поэтому таблицы сначала загружаются в память (ToList), а потом объединяются LINQ.
/// </summary>
[ApiController]
[Authorize]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ReportsController(AppDbContext db)
    {
        _db = db;
    }

    // 1. Добыча + цена Brent + бурение по месяцам (Production и DrillingActivity, связь по дате)
    // GET api/reports/monthly-summary?from=2022-01-01&to=2022-12-01
    [HttpGet("monthly-summary")]
    public IActionResult MonthlySummary(DateOnly? from, DateOnly? to)
    {
        var productions = _db.Productions
            .Where(p => (from == null || p.Date >= from) && (to == null || p.Date <= to))
            .ToList();
        var drillings = _db.DrillingActivities.ToList();

        var summary = productions
            .Join(drillings,
                production => production.Date, // по какому полю связываем первую таблицу
                drilling => drilling.Date,     // и вторую
                (production, drilling) => new  // что берём из пары
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

    // 2. Доли бассейнов за месяц и сравнение с национальной добычей нефти (BasinProduction и Production)
    // GET api/reports/basin-shares?date=2023-06-01
    [HttpGet("basin-shares")]
    public IActionResult BasinShares(DateOnly date)
    {
        var production = _db.Productions.FirstOrDefault(p => p.Date == date);
        var basinProductions = _db.BasinProductions.Where(b => b.Date == date).ToList();
        if (production == null || basinProductions.Count == 0)
            return NotFound(new { message = $"Нет данных за {date}" });

        var basinsTotal = basinProductions.Sum(b => b.Value);

        var shares = basinProductions
            .Select(b => new
            {
                b.Basin,
                b.BasinName,
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

    // 3. Рейтинг бассейнов за год с координатами: основа для карты в дипломе (BasinProduction и справочник Basins)
    // GET api/reports/top-basins?year=2023
    [HttpGet("top-basins")]
    public IActionResult TopBasins(int year)
    {
        var yearRecords = _db.BasinProductions
            .Where(b => b.Date >= new DateOnly(year, 1, 1) && b.Date <= new DateOnly(year, 12, 31))
            .ToList();
        if (yearRecords.Count == 0)
            return NotFound(new { message = $"Нет данных за {year} год" });

        var yearTotal = yearRecords.Sum(b => b.Value);

        var ranking = Basins.All
            .Select(basin =>
            {
                var basinTotal = yearRecords.Where(b => b.Basin == basin.Code).Sum(b => b.Value);
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
