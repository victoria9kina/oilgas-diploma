using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OilGasApi.Data;
using OilGasApi.Models;

namespace OilGasApi.Controllers;

[ApiController]
[Route("api/drilling")]//CRUD бурения
public class DrillingController : ControllerBase
{
    private readonly AppDbContext _db;

    public DrillingController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult GetAll(DateOnly? from, DateOnly? to, string? priceCategory)
    {
        var drillings = _db.DrillingActivities
            .Where(d => (from == null || d.Date >= from) && (to == null || d.Date <= to))
            .OrderBy(d => d.Date)
            .ToList();

        // категории цены нет в таблице, поэтому фильтруем после ToList
        if (priceCategory != null)
            drillings = drillings.Where(d => d.PriceCategory == priceCategory).ToList();

        return Ok(drillings);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var drilling = _db.DrillingActivities.Find(id);
        if (drilling == null)
            return NotFound();
        return Ok(drilling);
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public IActionResult Create(DrillingActivity drilling)
    {
        if (_db.DrillingActivities.Any(d => d.Date == drilling.Date))
            return Conflict(new { message = $"Запись за {drilling.Date} уже существует" });

        drilling.Id = 0; // id ставит база
        _db.DrillingActivities.Add(drilling);
        _db.SaveChanges();
        return CreatedAtAction(nameof(GetById), new { id = drilling.Id }, drilling);
    }

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
