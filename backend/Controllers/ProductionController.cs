using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OilGasApi.Data;
using OilGasApi.Models;

namespace OilGasApi.Controllers;

/// <summary>CRUD национальной добычи. Читать могут все, изменять только admin.</summary>
[ApiController]
[Route("api/production")]
public class ProductionController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductionController(AppDbContext db)
    {
        _db = db;
    }

    // GET api/production?from=2020-01-01&to=2020-12-01  (from и to необязательные)
    [HttpGet]
    public IActionResult GetAll(DateOnly? from, DateOnly? to)
    {
        var productions = _db.Productions
            .Where(p => (from == null || p.Date >= from) && (to == null || p.Date <= to))
            .OrderBy(p => p.Date)
            .ToList();
        return Ok(productions);
    }

    // GET api/production/5
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var production = _db.Productions.Find(id);
        if (production == null)
            return NotFound();
        return Ok(production);
    }

    // POST api/production
    [Authorize(Roles = "admin")]
    [HttpPost]
    public IActionResult Create(Production production)
    {
        if (_db.Productions.Any(p => p.Date == production.Date))
            return Conflict(new { message = $"Запись за {production.Date} уже существует" });

        production.Id = 0; // Id назначает база данных, а не клиент
        production.RecalculateTotals();
        _db.Productions.Add(production);
        _db.SaveChanges();
        return CreatedAtAction(nameof(GetById), new { id = production.Id }, production);
    }

    // PUT api/production/5
    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public IActionResult Update(int id, Production production)
    {
        if (!_db.Productions.Any(p => p.Id == id))
            return NotFound();
        if (_db.Productions.Any(p => p.Date == production.Date && p.Id != id))
            return Conflict(new { message = $"Запись за {production.Date} уже существует" });

        production.Id = id;
        production.RecalculateTotals();
        _db.Productions.Update(production);
        _db.SaveChanges();
        return Ok(production);
    }

    // DELETE api/production/5
    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var production = _db.Productions.Find(id);
        if (production == null)
            return NotFound();

        _db.Productions.Remove(production);
        _db.SaveChanges();
        return NoContent();
    }
}
