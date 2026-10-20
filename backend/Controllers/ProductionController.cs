using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OilGasApi.Auth;
using OilGasApi.Data;
using OilGasApi.Dtos;
using OilGasApi.Models;

namespace OilGasApi.Controllers;

/// <summary>CRUD национальной добычи. Чтение открыто всем, изменение только admin.</summary>
[ApiController]
[Route("api/production")]
public class ProductionController(AppDbContext db) : ControllerBase
{
    // GET api/production?from=2020-01-01&to=2020-12-01&minOil=1000
    [HttpGet]
    public async Task<IActionResult> GetAll(DateOnly? from, DateOnly? to, double? minOil)
    {
        var query = db.Productions.AsQueryable();
        if (from is not null) query = query.Where(p => p.Date >= from);
        if (to is not null) query = query.Where(p => p.Date <= to);
        if (minOil is not null) query = query.Where(p => p.OilTotal >= minOil);

        var list = await query.OrderBy(p => p.Date).ToListAsync();
        return Ok(list.Select(ToView));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await db.Productions.FindAsync(id);
        return p is null ? NotFound() : Ok(ToView(p));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create(ProductionDto dto)
    {
        var p = new Production();
        dto.ApplyTo(p);
        if (await db.Productions.AnyAsync(x => x.Date == p.Date))
            return Conflict(new { message = $"Запись за {p.Date:yyyy-MM} уже существует" });

        db.Productions.Add(p);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = p.Id }, ToView(p));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProductionDto dto)
    {
        var p = await db.Productions.FindAsync(id);
        if (p is null) return NotFound();

        dto.ApplyTo(p);
        if (await db.Productions.AnyAsync(x => x.Date == p.Date && x.Id != id))
            return Conflict(new { message = $"Запись за {p.Date:yyyy-MM} уже существует" });

        await db.SaveChangesAsync();
        return Ok(ToView(p));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await db.Productions.FindAsync(id);
        if (p is null) return NotFound();
        db.Productions.Remove(p);
        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Ответ API: данные + вычисляемые поля из бизнес-логики модели.</summary>
    private static object ToView(Production p) => new
    {
        p.Id,
        p.Date,
        p.OilConventional, p.OilShale, p.OilTight, p.OilTotal,
        p.GasConventional, p.GasShale, p.GasTight, p.GasTotal,
        OilUnconventional = p.OilUnconventional(),
        GasUnconventional = p.GasUnconventional(),
        OilUnconventionalSharePercent = p.OilUnconventionalSharePercent(),
        GasUnconventionalSharePercent = p.GasUnconventionalSharePercent(),
        IsConsistent = p.IsConsistent()
    };
}
