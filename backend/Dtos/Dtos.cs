using System.ComponentModel.DataAnnotations;
using OilGasApi.Models;

namespace OilGasApi.Dtos;

// DTO для создания/изменения записей. Id не принимаем от клиента,
// чтобы его нельзя было подменить в теле запроса.

public class ProductionDto
{
    [Required] public DateOnly Date { get; set; }

    [Range(0, double.MaxValue)] public double OilConventional { get; set; }
    [Range(0, double.MaxValue)] public double OilShale { get; set; }
    [Range(0, double.MaxValue)] public double OilTight { get; set; }

    [Range(0, double.MaxValue)] public double GasConventional { get; set; }
    [Range(0, double.MaxValue)] public double GasShale { get; set; }
    [Range(0, double.MaxValue)] public double GasTight { get; set; }

    /// <summary>Переносит данные в сущность. Итоги считаются моделью, а не приходят с клиента.</summary>
    public void ApplyTo(Production p)
    {
        p.Date = new DateOnly(Date.Year, Date.Month, 1);
        p.OilConventional = OilConventional;
        p.OilShale = OilShale;
        p.OilTight = OilTight;
        p.GasConventional = GasConventional;
        p.GasShale = GasShale;
        p.GasTight = GasTight;
        p.RecalculateTotals();
    }
}

public class BasinProductionDto
{
    [Required] public DateOnly Date { get; set; }

    [Required, StringLength(20)] public string Basin { get; set; } = "";

    [Range(0, double.MaxValue)] public double Value { get; set; }

    public void ApplyTo(BasinProduction b)
    {
        b.Date = new DateOnly(Date.Year, Date.Month, 1);
        b.Basin = Basin.ToLowerInvariant();
        b.Value = Value;
    }
}

public class DrillingActivityDto
{
    [Required] public DateOnly Date { get; set; }

    [Range(0, 500)] public double? BrentPrice { get; set; }
    [Range(0, double.MaxValue)] public double? MetersDrilled { get; set; }
    [Range(0, int.MaxValue)] public int? WellsCompleted { get; set; }
    [Range(0, int.MaxValue)] public int? ActiveUnconventionalWells { get; set; }

    public void ApplyTo(DrillingActivity d)
    {
        d.Date = new DateOnly(Date.Year, Date.Month, 1);
        d.BrentPrice = BrentPrice;
        d.MetersDrilled = MetersDrilled;
        d.WellsCompleted = WellsCompleted;
        d.ActiveUnconventionalWells = ActiveUnconventionalWells;
    }
}

public class LoginDto
{
    [Required] public string Username { get; set; } = "";
    [Required] public string Password { get; set; } = "";
}
