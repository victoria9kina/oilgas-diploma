using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OilGasApi.Auth;
using OilGasApi.Data;
using OilGasApi.Dtos;
using OilGasApi.Models;

namespace OilGasApi.Controllers;

/// <summary>CRUD добычи по бассейнам + справочник бассейнов с координатами.</summary>
[ApiController]
[Route("api/basins")]
public class BasinProductionController(AppDbContext db) : ControllerBase
{
    /// <summary>Справочник бассейнов (название, координаты центра): пригодится для карты.</summary>
    [HttpGet("info")]
    public IActionResult Info() => Ok(Basins.All);

    // GET api/basins?basin=neuquina&from=2020-01-01&to=2020-12-01
    [HttpGet]
    public async Task<IActionResult> GetAll(string? basin, DateOnly? from, DateOnly? to)
    {
        var query = db.BasinProductions.AsQueryable();
        if (!string.IsNullOrWhiteSpace(basin))
        {
            var code = basin.ToLowerInvariant();
            query = query.Where(b => b.Basin == code);
        }
        if (from is not null) query = query.Where(b => b.Date >= from);
        if (to is not null) query = query.Where(b => b.Date <= to);

        var list = await query.OrderBy(b => b.Date).ThenBy(b => b.Basin).ToListAsync();
        return Ok(list.Select(ToView));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var b = await db.BasinProductions.FindAsync(id);
        return b is null ? NotFound() : Ok(ToView(b));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create(BasinProductionDto dto)
    {
        if (!Basins.IsKnown(dto.Basin))
            return BadRequest(new { message = $"Неизвестный бассейн '{dto.Basin}'", allowed = Basins.All.Select(x => x.Code) });

        var b = new BasinProduction();
        dto.ApplyTo(b);
        if (await db.BasinProductions.AnyAsync(x => x.Date == b.Date && x.Basin == b.Basin))
            return Conflict(new { message = $"Запись {b.Basin} за {b.Date:yyyy-MM} уже существует" });

        db.BasinProductions.Add(b);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = b.Id }, ToView(b));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, BasinProductionDto dto)
    {
        if (!Basins.IsKnown(dto.Basin))
            return BadRequest(new { message = $"Неизвестный бассейн '{dto.Basin}'" });

        var b = await db.BasinProductions.FindAsync(id);
        if (b is null) return NotFound();

        dto.ApplyTo(b);
        if (await db.BasinProductions.AnyAsync(x => x.Date == b.Date && x.Basin == b.Basin && x.Id != id))
            return Conflict(new { message = $"Запись {b.Basin} за {b.Date:yyyy-MM} уже существует" });

        await db.SaveChangesAsync();
        return Ok(ToView(b));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var b = await db.BasinProductions.FindAsync(id);
        if (b is null) return NotFound();
        db.BasinProductions.Remove(b);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static object ToView(BasinProduction b)
    {
        var info = b.Info();
        return new
        {
            b.Id,
            b.Date,
            b.Basin,
            BasinName = info?.Name,
            info?.Latitude,
            info?.Longitude,
            b.Value
        };
    }
}
