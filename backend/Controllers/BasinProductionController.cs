using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OilGasApi.Data;
using OilGasApi.Models;

namespace OilGasApi.Controllers;

/// <summary>CRUD добычи по бассейнам и справочник бассейнов. Читать могут все, изменять только admin.</summary>
[ApiController]
[Route("api/basins")]
public class BasinProductionController : ControllerBase
{
    private readonly AppDbContext _db;

    public BasinProductionController(AppDbContext db)
    {
        _db = db;
    }

    // GET api/basins/info: справочник бассейнов с координатами
    [HttpGet("info")]
    public IActionResult GetInfo()
    {
        return Ok(Basins.All);
    }

    // GET api/basins?basin=neuquina&from=2023-01-01&to=2023-12-01  (все параметры необязательные)
    [HttpGet]
    public IActionResult GetAll(string? basin, DateOnly? from, DateOnly? to)
    {
        var basinProductions = _db.BasinProductions
            .Where(b => (basin == null || b.Basin == basin)
                     && (from == null || b.Date >= from)
                     && (to == null || b.Date <= to))
            .OrderBy(b => b.Date)
            .ThenBy(b => b.Basin)
            .ToList();
        return Ok(basinProductions);
    }

    // GET api/basins/5
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var basinProduction = _db.BasinProductions.Find(id);
        if (basinProduction == null)
            return NotFound();
        return Ok(basinProduction);
    }

    // POST api/basins
    [Authorize(Roles = "admin")]
    [HttpPost]
    public IActionResult Create(BasinProduction basinProduction)
    {
        if (!Basins.IsKnown(basinProduction.Basin))
            return BadRequest(new { message = $"Неизвестный бассейн {basinProduction.Basin}" });
        if (_db.BasinProductions.Any(b => b.Date == basinProduction.Date && b.Basin == basinProduction.Basin))
            return Conflict(new { message = $"Запись {basinProduction.Basin} за {basinProduction.Date} уже существует" });

        basinProduction.Id = 0; // Id назначает база данных, а не клиент
        _db.BasinProductions.Add(basinProduction);
        _db.SaveChanges();
        return CreatedAtAction(nameof(GetById), new { id = basinProduction.Id }, basinProduction);
    }

    // PUT api/basins/5
    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public IActionResult Update(int id, BasinProduction basinProduction)
    {
        if (!_db.BasinProductions.Any(b => b.Id == id))
            return NotFound();
        if (!Basins.IsKnown(basinProduction.Basin))
            return BadRequest(new { message = $"Неизвестный бассейн {basinProduction.Basin}" });
        if (_db.BasinProductions.Any(b => b.Date == basinProduction.Date && b.Basin == basinProduction.Basin && b.Id != id))
            return Conflict(new { message = $"Запись {basinProduction.Basin} за {basinProduction.Date} уже существует" });

        basinProduction.Id = id;
        _db.BasinProductions.Update(basinProduction);
        _db.SaveChanges();
        return Ok(basinProduction);
    }

    // DELETE api/basins/5
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
