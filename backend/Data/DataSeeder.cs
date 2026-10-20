using System.Globalization;
using OilGasApi.Models;

namespace OilGasApi.Data;

/// <summary>
/// Первичное заполнение БД из CSV (Argentina Oil and Gas Dataset).
/// Одна строка CSV = один месяц = 1 Production + 5 BasinProduction + 1 DrillingActivity.
/// </summary>
public static class DataSeeder
{
    public static void Seed(AppDbContext db, string csvPath)
    {
        db.Database.EnsureCreated(); // создаёт файл БД и таблицы, если их ещё нет

        if (db.Productions.Any())
            return; // данные уже загружены при прошлом запуске

        var lines = File.ReadAllLines(csvPath);
        var header = lines[0].Split(',');

        foreach (var line in lines.Skip(1))
        {
            var cells = line.Split(',');
            var date = DateOnly.Parse(cells[0], CultureInfo.InvariantCulture); // колонка fecha

            db.Productions.Add(new Production
            {
                Date = date,
                OilConventional = ReadNumber(header, cells, "petroleo_convencional"),
                OilShale = ReadNumber(header, cells, "petroleo_shale"),
                OilTight = ReadNumber(header, cells, "petroleo_tight"),
                OilTotal = ReadNumber(header, cells, "petroleo_total"),
                GasConventional = ReadNumber(header, cells, "gas_convencional"),
                GasShale = ReadNumber(header, cells, "gas_shale"),
                GasTight = ReadNumber(header, cells, "gas_tight"),
                GasTotal = ReadNumber(header, cells, "gas_total")
            });

            // Каждая колонка cuenca_* превращается в отдельную запись по бассейну
            foreach (var basin in Basins.All)
            {
                db.BasinProductions.Add(new BasinProduction
                {
                    Date = date,
                    Basin = basin.Code,
                    Value = ReadNumber(header, cells, basin.CsvColumn)
                });
            }

            db.DrillingActivities.Add(new DrillingActivity
            {
                Date = date,
                BrentPrice = ReadNumber(header, cells, "precio_brent"),
                MetersDrilled = ReadNumber(header, cells, "metros_perforados"),
                // В CSV скважины записаны дробью ("40.0"), поэтому читаем как число и приводим к int
                WellsCompleted = (int)ReadNumber(header, cells, "pozos_terminados"),
                ActiveUnconventionalWells = (int)ReadNumber(header, cells, "pozos_no_conv_activos")
            });
        }

        db.SaveChanges();
        Console.WriteLine($"Импортировано месяцев из CSV: {lines.Length - 1}");
    }

    /// <summary>Прочитать число из ячейки по названию колонки.
    /// InvariantCulture нужна, чтобы точка считалась десятичным разделителем на любой системе.</summary>
    private static double ReadNumber(string[] header, string[] cells, string column)
    {
        int index = Array.IndexOf(header, column);
        return double.Parse(cells[index], CultureInfo.InvariantCulture);
    }
}
