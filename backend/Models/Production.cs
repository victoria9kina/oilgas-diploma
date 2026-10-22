using System.ComponentModel.DataAnnotations;

namespace OilGasApi.Models;

public class Production
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    [Range(0, double.MaxValue)] public double OilConventional { get; set; }
    [Range(0, double.MaxValue)] public double OilShale { get; set; }
    [Range(0, double.MaxValue)] public double OilTight { get; set; }
    public double OilTotal { get; set; }

    [Range(0, double.MaxValue)] public double GasConventional { get; set; }
    [Range(0, double.MaxValue)] public double GasShale { get; set; }
    [Range(0, double.MaxValue)] public double GasTight { get; set; }
    public double GasTotal { get; set; }

    // поля ниже считаются из полей выше, в базе не хранятся
    // нетрадиционная добыча: сланцевая + tight
    public double OilUnconventional => OilShale + OilTight;

    public double GasUnconventional => GasShale + GasTight;

    // доля нетрадиционной добычи, %
    public double OilUnconventionalSharePercent => Percent.Of(OilUnconventional, OilTotal);

    public double GasUnconventionalSharePercent => Percent.Of(GasUnconventional, GasTotal);

    // итог всегда равен сумме видов добычи
    public void RecalculateTotals()
    {
        OilTotal = OilConventional + OilShale + OilTight;
        GasTotal = GasConventional + GasShale + GasTight;
    }
}
