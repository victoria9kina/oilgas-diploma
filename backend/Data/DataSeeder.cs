using System.Globalization;
using OilGasApi.Models;

namespace OilGasApi.Data;

public static class DataSeeder
{
    public static void Seed(AppDbContext db, string dataFolder)
    {
        db.Database.EnsureCreated(); //чекает есть ли база, если нет, то создает с этими таблицами

        if (db.Productions.Any())
            return; // данные уже загружены

        // справочник бассейнов
        var basins = new List<Basin>
        {
            new Basin { Code = "neuquina", Name = "Неукен", Latitude = -38.5, Longitude = -69.0 },
            new Basin { Code = "gsj", Name = "Залив Сан-Хорхе", Latitude = -45.9, Longitude = -68.3 },
            new Basin { Code = "austral", Name = "Аустраль", Latitude = -51.6, Longitude = -69.4 },
            new Basin { Code = "noroeste", Name = "Северо-Западный", Latitude = -22.9, Longitude = -63.8 },
            new Basin { Code = "cuyana", Name = "Куйо", Latitude = -33.3, Longitude = -68.6 }
        };
        db.Basins.AddRange(basins);

        var lines = File.ReadAllLines(Path.Combine(dataFolder, "argentina_oil_gas.csv"));
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
            foreach (var basin in basins)
            {
                db.BasinProductions.Add(new BasinProduction
                {
                    Date = date,
                    Basin = basin, // BasinId база проставит сама при сохранении
                    Value = ReadNumber(header, cells, $"cuenca_{basin.Code}")
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

        SeedMarketFactors(db, Path.Combine(dataFolder, "market_factors.csv"));
        SeedEvents(db, Path.Combine(dataFolder, "market_events.csv"));

        db.SaveChanges();
        Console.WriteLine($"Импортировано месяцев из CSV: {lines.Length - 1}");
    }

    // экспорт курс и спрос собраны из datos.gob.ar и EIA по тем же 203 месяцам
    private static void SeedMarketFactors(AppDbContext db, string csvPath)
    {
        var lines = File.ReadAllLines(csvPath);
        var header = lines[0].Split(',');

        foreach (var line in lines.Skip(1))
        {
            var cells = line.Split(',');
            db.MarketFactors.Add(new MarketFactor
            {
                Date = DateOnly.Parse(ReadText(header, cells, "date"), CultureInfo.InvariantCulture),
                OilExportUsdMln = ReadNumber(header, cells, "oil_export_usd_mln"),
                GasExportUsdMln = ReadNumber(header, cells, "gas_export_usd_mln"),
                UsdArsRate = ReadNumber(header, cells, "usd_ars_rate"),
                WorldOilDemand = ReadNumber(header, cells, "world_oil_demand")
            });
        }
    }

    private static void SeedEvents(AppDbContext db, string csvPath)
    {
        var lines = File.ReadAllLines(csvPath);
        var header = lines[0].Split(',');

        foreach (var line in lines.Skip(1))
        {
            var cells = line.Split(',');
            db.MarketEvents.Add(new MarketEvent
            {
                Date = DateOnly.Parse(ReadText(header, cells, "date"), CultureInfo.InvariantCulture),
                Title = ReadText(header, cells, "title"),
                Category = ReadText(header, cells, "category"),
                Impact = (int)ReadNumber(header, cells, "impact"),
                SourceUrl = ReadText(header, cells, "source_url")
            });
        }
    }

    // ищем колонку по названию и берём из неё текст
    private static string ReadText(string[] header, string[] cells, string column)
    {
        int index = Array.IndexOf(header, column);
        return cells[index];
    }

    private static double ReadNumber(string[] header, string[] cells, string column)
    {
        // InvariantCulture значит что дробь пишется через точку
        return double.Parse(ReadText(header, cells, column), CultureInfo.InvariantCulture);
    }
}
