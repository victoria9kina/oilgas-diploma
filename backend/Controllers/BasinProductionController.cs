using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OilGasApi.Data;
using OilGasApi.Models;

namespace OilGasApi.Controllers;

[ApiController]
[Route("api/basins")]//CRUD добычи по бассейнам
public class BasinProductionController : ControllerBase
{
    private readonly AppDbContext _db;

    public BasinProductionController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("info")]
    public IActionResult GetInfo()
    {
        return Ok(_db.Basins.ToList());
    }

    [HttpGet]
    public IActionResult GetAll(int? basinId, DateOnly? from, DateOnly? to)
    {
        var basinProductions = _db.BasinProductions
            .Include(b => b.Basin) // подгружаем бассейн из второй таблицы
            .Where(b => (basinId == null || b.BasinId == basinId)
                     && (from == null || b.Date >= from)
                     && (to == null || b.Date <= to))
            .OrderBy(b => b.Date)
            .ThenBy(b => b.BasinId)
            .ToList();
        return Ok(basinProductions);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var basinProduction = _db.BasinProductions.Include(b => b.Basin).FirstOrDefault(b => b.Id == id);
        if (basinProduction == null)
            return NotFound();
        return Ok(basinProduction);
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public IActionResult Create(BasinProduction basinProduction)
    {
        // такого бассейна нет в таблице бассейнов
        if (!_db.Basins.Any(b => b.Id == basinProduction.BasinId))
            return BadRequest(new { message = $"Нет бассейна с id {basinProduction.BasinId}" });
        if (_db.BasinProductions.Any(b => b.Date == basinProduction.Date && b.BasinId == basinProduction.BasinId))
            return Conflict(new { message = $"Запись бассейна {basinProduction.BasinId} за {basinProduction.Date} уже существует" });

        basinProduction.Id = 0; // id ставит база
        _db.BasinProductions.Add(basinProduction);
        _db.SaveChanges();
        return CreatedAtAction(nameof(GetById), new { id = basinProduction.Id }, basinProduction);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public IActionResult Update(int id, BasinProduction basinProduction)
    {
        if (!_db.BasinProductions.Any(b => b.Id == id))
            return NotFound();
        if (!_db.Basins.Any(b => b.Id == basinProduction.BasinId))
            return BadRequest(new { message = $"Нет бассейна с id {basinProduction.BasinId}" });
        if (_db.BasinProductions.Any(b => b.Date == basinProduction.Date && b.BasinId == basinProduction.BasinId && b.Id != id))
            return Conflict(new { message = $"Запись бассейна {basinProduction.BasinId} за {basinProduction.Date} уже существует" });

        basinProduction.Id = id;
        _db.BasinProductions.Update(basinProduction);
        _db.SaveChanges();
        return Ok(basinProduction);
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var basinProduction = _db.BasinProductions.Find(id);
        if (basinProduction == null)
            return NotFound();

        _db.BasinProductions.Remove(basinProduction);
        _db.SaveChanges();
        return NoContent();
    }
}
