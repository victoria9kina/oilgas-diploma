using Microsoft.EntityFrameworkCore;

namespace OilGasApi.Data;

/// <summary>Создаёт БД при первом запуске и заполняет её из CSV, если таблицы пустые.</summary>
public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext db, string csvPath, ILogger logger)
    {
        await db.Database.EnsureCreatedAsync();

        if (await db.Productions.AnyAsync())
        {
            logger.LogInformation("БД уже заполнена, импорт пропущен");
            return;
        }

        if (!File.Exists(csvPath))
        {
            logger.LogWarning("CSV не найден по пути {Path}. БД пустая, данные можно добавить через API.", csvPath);
            return;
        }

        var parsed = CsvDatasetParser.Parse(await File.ReadAllLinesAsync(csvPath));

        // Если в CSV вдруг есть повторяющиеся месяцы, берём последнюю строку (иначе упадёт уникальный индекс)
        db.Productions.AddRange(parsed.Productions.GroupBy(p => p.Date).Select(g => g.Last()));
        db.BasinProductions.AddRange(parsed.Basins.GroupBy(b => new { b.Date, b.Basin }).Select(g => g.Last()));
        db.DrillingActivities.AddRange(parsed.Drilling.GroupBy(d => d.Date).Select(g => g.Last()));
        await db.SaveChangesAsync();

        logger.LogInformation("Импортировано: {P} месяцев добычи, {B} записей по бассейнам, {D} записей бурения",
            parsed.Productions.Count, parsed.Basins.Count, parsed.Drilling.Count);
    }
}
