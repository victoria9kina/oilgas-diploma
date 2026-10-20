using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OilGasApi.Auth;
using OilGasApi.Data;
using OilGasApi.Dtos;
using OilGasApi.Models;

namespace OilGasApi.Controllers;

/// <summary>CRUD буровой активности и цены Brent.</summary>
[ApiController]
[Route("api/drilling")]
public class DrillingController(AppDbContext db) : ControllerBase
{
    // GET api/drilling?from=2020-01-01&to=2020-12-01&priceCategory=High
    [HttpGet]
    public async Task<IActionResult> GetAll(DateOnly? from, DateOnly? to, string? priceCategory)
    {
        var query = db.DrillingActivities.AsQueryable();
        if (from is not null) query = query.Where(d => d.Date >= from);
        if (to is not null) query = query.Where(d => d.Date <= to);

        var list = await query.OrderBy(d => d.Date).ToListAsync();

        // Категория цены считается методом модели, поэтому фильтруем уже в памяти (LINQ to Objects)
        if (!string.IsNullOrWhiteSpace(priceCategory))
            list = list.Where(d => d.PriceCategory().Equals(priceCategory, StringComparison.OrdinalIgnoreCase)).ToList();

        return Ok(list.Select(ToView));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var d = await db.DrillingActivities.FindAsync(id);
        return d is null ? NotFound() : Ok(ToView(d));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create(DrillingActivityDto dto)
    {
        var d = new DrillingActivity();
        dto.ApplyTo(d);
        if (await db.DrillingActivities.AnyAsync(x => x.Date == d.Date))
            return Conflict(new { message = $"Запись за {d.Date:yyyy-MM} уже существует" });

        db.DrillingActivities.Add(d);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = d.Id }, ToView(d));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, DrillingActivityDto dto)
    {
        var d = await db.DrillingActivities.FindAsync(id);
        if (d is null) return NotFound();

        dto.ApplyTo(d);
        if (await db.DrillingActivities.AnyAsync(x => x.Date == d.Date && x.Id != id))
            return Conflict(new { message = $"Запись за {d.Date:yyyy-MM} уже существует" });

        await db.SaveChangesAsync();
        return Ok(ToView(d));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var d = await db.DrillingActivities.FindAsync(id);
        if (d is null) return NotFound();
        db.DrillingActivities.Remove(d);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static object ToView(DrillingActivity d) => new
    {
        d.Id,
        d.Date,
        d.BrentPrice,
        d.MetersDrilled,
        d.WellsCompleted,
        d.ActiveUnconventionalWells,
        MetersPerWell = d.MetersPerWell(),
        PriceCategory = d.PriceCategory(),
        HasDrilling = d.HasDrilling()
    };
}
