namespace OilGasApi.Models;

/// <summary>Справочные данные о бассейне. Координаты: примерный центр бассейна, нужны для карты в дипломе.</summary>
public record BasinInfo(string Code, string Name, string CsvColumn, double Latitude, double Longitude);

/// <summary>Справочник нефтегазовых бассейнов Аргентины. Он не меняется, поэтому хранится в коде, а не в БД.</summary>
public static class Basins
{
    public static readonly List<BasinInfo> All = new List<BasinInfo>
    {
        new BasinInfo("neuquina", "Неукен",          "cuenca_neuquina", -38.5, -69.0),
        new BasinInfo("gsj",      "Залив Сан-Хорхе", "cuenca_gsj",      -45.9, -68.3),
        new BasinInfo("austral",  "Аустраль",        "cuenca_austral",  -51.6, -69.4),
        new BasinInfo("noroeste", "Северо-Западный", "cuenca_noroeste", -22.9, -63.8),
        new BasinInfo("cuyana",   "Куйо",            "cuenca_cuyana",   -33.3, -68.6),
    };

    /// <summary>Найти бассейн по коду. Если такого нет, вернёт null.</summary>
    public static BasinInfo? Find(string code) => All.FirstOrDefault(b => b.Code == code);

    public static bool IsKnown(string code) => Find(code) != null;
}
