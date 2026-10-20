namespace OilGasApi.Models;

/// <summary>Справочная информация о бассейне. Координаты: приблизительный центр, для будущей карты.</summary>
public record BasinInfo(string Code, string Name, string NameRu, string CsvColumn, double Latitude, double Longitude);

/// <summary>Справочник нефтегазовых бассейнов Аргентины.</summary>
public static class Basins
{
    public static readonly IReadOnlyList<BasinInfo> All = new List<BasinInfo>
    {
        new("neuquina", "Cuenca Neuquina",       "Неукенский бассейн",       "cuenca_neuquina", -38.5, -69.0),
        new("gsj",      "Cuenca Golfo San Jorge", "Бассейн залива Сан-Хорхе", "cuenca_gsj",      -45.9, -68.3),
        new("austral",  "Cuenca Austral",         "Южный (Аустраль) бассейн", "cuenca_austral",  -51.6, -69.4),
        new("noroeste", "Cuenca Noroeste",        "Северо-Западный бассейн",  "cuenca_noroeste", -22.9, -63.8),
        new("cuyana",   "Cuenca Cuyana",          "Куйянский бассейн",        "cuenca_cuyana",   -33.3, -68.6),
    };

    public static BasinInfo? Find(string code) =>
        All.FirstOrDefault(b => b.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

    public static bool IsKnown(string code) => Find(code) is not null;
}
