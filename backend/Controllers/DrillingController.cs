using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OilGasApi.Data;
using OilGasApi.Models;

namespace OilGasApi.Controllers;

/// <summary>CRUD буровой активности и цены Brent. Читать могут все, изменять только admin.</summary>
[ApiController]
[Route("api/drilling")]
public class DrillingController : ControllerBase
{
    private readonly AppDbContext _db;

    public DrillingController(AppDbContext db)
    {
        _db = db;
    }

    // GET api/drilling?from=2020-01-01&to=2020-12-01&priceCategory=High  (все параметры необязательные)
    [HttpGet]
    public IActionResult GetAll(DateOnly? from, DateOnly? to, string? priceCategory)
    {
        var drillings = _db.DrillingActivities
            .Where(d => (from == null || d.Date >= from) && (to == null || d.Date <= to))
            .OrderBy(d => d.Date)
            .ToList();

        // PriceCategory вычисляется в C#, в таблице такой колонки нет и SQL о ней не знает.
        // Поэтому фильтруем уже загруженный список.
        if (priceCategory != null)
            drillings = drillings.Where(d => d.PriceCategory == priceCategory).ToList();

        return Ok(drillings);
    }

    // GET api/drilling/5
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var drilling = _db.DrillingActivities.Find(id);
        if (drilling == null)
            return NotFound();
        return Ok(drilling);
    }

    // POST api/drilling
    [Authorize(Roles = "admin")]
    [HttpPost]
    public IActionResult Create(DrillingActivity drilling)
    {
        if (_db.DrillingActivities.Any(d => d.Date == drilling.Date))
            return Conflict(new { message = $"Запись за {drilling.Date} уже существует" });

        drilling.Id = 0; // Id назначает база данных, а не клиент
        _db.DrillingActivities.Add(drilling);
        _db.SaveChanges();
        return CreatedAtAction(nameof(GetById), new { id = drilling.Id }, drilling);
    }

    // PUT api/drilling/5
    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public IActionResult Update(int id, DrillingActivity drilling)
    {
        if (!_db.DrillingActivities.Any(d => d.Id == id))
            return NotFound();
        if (_db.DrillingActivities.Any(d => d.Date == drilling.Date && d.Id != id))
            return Conflict(new { message = $"Запись за {drilling.Date} уже существует" });

        drilling.Id = id;
        _db.DrillingActivities.Update(drilling);
        _db.SaveChanges();
        return Ok(drilling);
    }

    // DELETE api/drilling/5
    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var drilling = _db.DrillingActivities.Find(id);
        if (drilling == null)
            return NotFound();

        _db.DrillingActivities.Remove(drilling);
        _db.SaveChanges();
        return NoContent();
    }
}
