using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OilGasApi.Data;
using OilGasApi.Models;

namespace OilGasApi.Controllers;

// отчёты только для вошедших пользователей
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
                production => production.Date,
                drilling => drilling.Date,
                (production, drilling) => new
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

    // рейтинг бассейнов с координатами: основа карты в дипломе
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
