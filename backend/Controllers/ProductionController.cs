using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OilGasApi.Data;
using OilGasApi.Models;

namespace OilGasApi.Controllers;

[ApiController]
[Route("api/production")]//CRUD добычи по стране
public class ProductionController : ControllerBase
{
    private readonly AppDbContext _db;

    // базу передаёт сам asp net через конструктор
    public ProductionController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult GetAll(DateOnly? from, DateOnly? to)
    {
        // если дата не передана то условие не мешает
        var productions = _db.Productions
            .Where(p => (from == null || p.Date >= from) && (to == null || p.Date <= to))
            .OrderBy(p => p.Date)
            .ToList();
        return Ok(productions);
    }

//GET-запрос по адресу api/production/max-oil, возвращает запись с максимальной добычей нефти
    [HttpGet("max-oil")] //адрес метода
    public IActionResult GetMaxOil()
    {
        var production = _db.Productions.OrderByDescending(p => p.OilTotal).FirstOrDefault();
        if (production == null)
            return NotFound();
        return Ok(production);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var production = _db.Productions.Find(id);
        if (production == null)
            return NotFound();
        return Ok(production);
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public IActionResult Create(Production production) //сюда приходят данные из postman
    {
        // за один месяц только одна запись
        if (_db.Productions.Any(p => p.Date == production.Date))
            return Conflict(new { message = $"Запись за {production.Date} уже существует" });

        production.Id = 0; // id ставит база
        production.RecalculateTotals(); // пересчитываем итог нефти и газа
        _db.Productions.Add(production);
        _db.SaveChanges();
        // ответ 201 и адрес новой записи
        return CreatedAtAction(nameof(GetById), new { id = production.Id }, production);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public IActionResult Update(int id, Production production)
    {
        if (!_db.Productions.Any(p => p.Id == id))
            return NotFound();
        // другой записи с таким месяцем быть не должно
        if (_db.Productions.Any(p => p.Date == production.Date && p.Id != id))
            return Conflict(new { message = $"Запись за {production.Date} уже существует" });

        production.Id = id;
        production.RecalculateTotals();
        _db.Productions.Update(production);
        _db.SaveChanges();
        return Ok(production);
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var production = _db.Productions.Find(id);
        if (production == null)
            return NotFound();

        _db.Productions.Remove(production);
        _db.SaveChanges();
        return NoContent(); // 204 удалили и ничего не возвращаем
    }
}
