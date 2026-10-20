using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OilGasApi.Data;
using OilGasApi.Services;

namespace OilGasApi.Controllers;

/// <summary>
/// Сводные отчёты (объединение данных из разных классов).
/// Доступны только авторизованным пользователям (любая роль): без токена -> 401.
/// </summary>
[ApiController]
[Authorize]
[Route("api/reports")]
public class ReportsController(AppDbContext db) : ControllerBase
{
    /// <summary>1. Помесячно: добыча + цена Brent + бурение.</summary>
    [HttpGet("monthly-summary")]
    public async Task<IActionResult> MonthlySummary(DateOnly? from, DateOnly? to)
    {
        var productions = await db.Productions.AsNoTracking().ToListAsync();
        var drilling = await db.DrillingActivities.AsNoTracking().ToListAsync();
        return Ok(ReportService.MonthlySummaries(productions, drilling, from, to));
    }

    /// <summary>2. Доли бассейнов за конкретный месяц + сравнение с национальной добычей.</summary>
    [HttpGet("basin-shares")]
    public async Task<IActionResult> BasinShares([FromQuery] DateOnly date)
    {
        var month = new DateOnly(date.Year, date.Month, 1);
        var basins = await db.BasinProductions.AsNoTracking().Where(b => b.Date == month).ToListAsync();
        var productions = await db.Productions.AsNoTracking().Where(p => p.Date == month).ToListAsync();

        var report = ReportService.BasinShares(basins, productions, month);
        return report is null ? NotFound(new { message = $"Нет данных за {month:yyyy-MM}" }) : Ok(report);
    }

    /// <summary>3. Годовая сводка: средняя добыча, прирост, средний Brent, скважины.</summary>
    [HttpGet("yearly")]
    public async Task<IActionResult> Yearly()
    {
        var productions = await db.Productions.AsNoTracking().ToListAsync();
        var drilling = await db.DrillingActivities.AsNoTracking().ToListAsync();
        return Ok(ReportService.Yearly(productions, drilling));
    }

    /// <summary>4. Как цена Brent связана с добычей и бурением.</summary>
    [HttpGet("price-impact")]
    public async Task<IActionResult> PriceImpact()
    {
        var productions = await db.Productions.AsNoTracking().ToListAsync();
        var drilling = await db.DrillingActivities.AsNoTracking().ToListAsync();
        return Ok(ReportService.PriceImpactReport(productions, drilling));
    }

    /// <summary>5. Рейтинг бассейнов за год с координатами (основа для геовизуализации).</summary>
    [HttpGet("top-basins")]
    public async Task<IActionResult> TopBasins([FromQuery] int year)
    {
        var start = new DateOnly(year - 1, 1, 1);
        var end = new DateOnly(year, 12, 31);
        var basins = await db.BasinProductions.AsNoTracking()
            .Where(b => b.Date >= start && b.Date <= end)
            .ToListAsync();
        var result = ReportService.TopBasins(basins, year);
        return result.Count == 0 ? NotFound(new { message = $"Нет данных за {year} год" }) : Ok(result);
    }
}
