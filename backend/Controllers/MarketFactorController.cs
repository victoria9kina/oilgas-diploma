using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OilGasApi.Data;
using OilGasApi.Models;

namespace OilGasApi.Controllers;

[ApiController]
[Route("api/market-factors")]
public class MarketFactorController : ControllerBase
{
    private readonly AppDbContext _db;

    public MarketFactorController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult GetAll(DateOnly? from, DateOnly? to)
    {
        var marketFactors = _db.MarketFactors
            .Where(m => (from == null || m.Date >= from) && (to == null || m.Date <= to))
            .OrderBy(m => m.Date)
            .ToList();
        return Ok(marketFactors);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var marketFactor = _db.MarketFactors.Find(id);
        if (marketFactor == null)
            return NotFound();
        return Ok(marketFactor);
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public IActionResult Create(MarketFactor marketFactor)
    {
        if (_db.MarketFactors.Any(m => m.Date == marketFactor.Date))
            return Conflict(new { message = $"Запись за {marketFactor.Date} уже существует" });

        marketFactor.Id = 0; // id ставит база
        _db.MarketFactors.Add(marketFactor);
        _db.SaveChanges();
        return CreatedAtAction(nameof(GetById), new { id = marketFactor.Id }, marketFactor);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public IActionResult Update(int id, MarketFactor marketFactor)
    {
        if (!_db.MarketFactors.Any(m => m.Id == id))
            return NotFound();
        if (_db.MarketFactors.Any(m => m.Date == marketFactor.Date && m.Id != id))
            return Conflict(new { message = $"Запись за {marketFactor.Date} уже существует" });

        marketFactor.Id = id;
        _db.MarketFactors.Update(marketFactor);
        _db.SaveChanges();
        return Ok(marketFactor);
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var marketFactor = _db.MarketFactors.Find(id);
        if (marketFactor == null)
            return NotFound();

        _db.MarketFactors.Remove(marketFactor);
        _db.SaveChanges();
        return NoContent();
    }
}
