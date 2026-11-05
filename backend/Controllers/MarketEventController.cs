using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OilGasApi.Data;
using OilGasApi.Models;

namespace OilGasApi.Controllers;

[ApiController]
[Route("api/events")]
public class MarketEventController : ControllerBase
{
    private readonly AppDbContext _db;

    public MarketEventController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult GetAll(DateOnly? from, DateOnly? to, string? category)
    {
        var marketEvents = _db.MarketEvents
            .Where(e => (from == null || e.Date >= from)
                     && (to == null || e.Date <= to)
                     && (category == null || e.Category == category))
            .OrderBy(e => e.Date)
            .ToList();
        return Ok(marketEvents);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var marketEvent = _db.MarketEvents.Find(id);
        if (marketEvent == null)
            return NotFound();
        return Ok(marketEvent);
    }

    // в одном месяце может быть несколько событий поэтому проверки на дубль нет
    [Authorize(Roles = "admin")]
    [HttpPost]
    public IActionResult Create(MarketEvent marketEvent)
    {
        marketEvent.Id = 0; // id ставит база
        _db.MarketEvents.Add(marketEvent);
        _db.SaveChanges();
        return CreatedAtAction(nameof(GetById), new { id = marketEvent.Id }, marketEvent);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public IActionResult Update(int id, MarketEvent marketEvent)
    {
        if (!_db.MarketEvents.Any(e => e.Id == id))
            return NotFound();

        marketEvent.Id = id;
        _db.MarketEvents.Update(marketEvent);
        _db.SaveChanges();
        return Ok(marketEvent);
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var marketEvent = _db.MarketEvents.Find(id);
        if (marketEvent == null)
            return NotFound();

        _db.MarketEvents.Remove(marketEvent);
        _db.SaveChanges();
        return NoContent();
    }
}
