using System.Globalization;
using OilGasApi.Models;

namespace OilGasApi.Data;

public static class DataSeeder
{
    public static void Seed(AppDbContext db, string csvPath)
    {
        db.Database.EnsureCreated(); //чекает есть ли база, если нет, то создает с этими таблицами

        if (db.Productions.Any())
            return; // данные уже загружены

        var lines = File.ReadAllLines(csvPath);
        var header = lines[0].Split(','); // первая строка это названия колонок

        foreach (var line in lines.Skip(1))
        {
            var cells = line.Split(',');
            var date = DateOnly.Parse(cells[0], CultureInfo.InvariantCulture);

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

            // каждая колонка cuenca_* становится отдельной записью
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
                // в csv скважины записаны дробным числом поэтому приводим к int
                WellsCompleted = (int)ReadNumber(header, cells, "pozos_terminados"),
                ActiveUnconventionalWells = (int)ReadNumber(header, cells, "pozos_no_conv_activos")
            });
        }

        db.SaveChanges();
        Console.WriteLine($"Импортировано месяцев из CSV: {lines.Length - 1}");
    }

    // ищем колонку по названию и берём из неё число
    private static double ReadNumber(string[] header, string[] cells, string column)
    {
        // InvariantCulture значит что дробь пишется через точку
        int index = Array.IndexOf(header, column);
        return double.Parse(cells[index], CultureInfo.InvariantCulture);
    }
}
