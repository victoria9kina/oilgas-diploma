using System.ComponentModel.DataAnnotations;

namespace OilGasApi.Models;

// рыночные факторы за месяц для прогноза
public class MarketFactor
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    // экспорт нефти и газа в млн долларов
    [Range(0, double.MaxValue)] public double OilExportUsdMln { get; set; }
    [Range(0, double.MaxValue)] public double GasExportUsdMln { get; set; }

    // сколько песо стоит один доллар
    [Range(0, double.MaxValue)] public double UsdArsRate { get; set; }

    // мировой спрос на нефть в млн баррелей в сутки
    [Range(0, double.MaxValue)] public double WorldOilDemand { get; set; }

    // весь экспорт нефть плюс газ
    public double TotalExportUsdMln => OilExportUsdMln + GasExportUsdMln;

    // доля нефти в экспорте в процентах
    public double OilExportSharePercent => Percent.Of(OilExportUsdMln, TotalExportUsdMln);
}
