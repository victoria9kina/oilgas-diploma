using System.ComponentModel.DataAnnotations;

namespace OilGasApi.Models;

/// <summary>Цена нефти Brent и буровая активность за месяц.</summary>
public class DrillingActivity
{
    public int Id { get; set; }

    /// <summary>Месяц (первое число месяца).</summary>
    public DateOnly Date { get; set; }

    /// <summary>Цена Brent, долларов за баррель (precio_brent).</summary>
    [Range(0, 500)] public double BrentPrice { get; set; }

    /// <summary>Пробурено метров (metros_perforados).</summary>
    [Range(0, double.MaxValue)] public double MetersDrilled { get; set; }

    /// <summary>Законченных скважин (pozos_terminados).</summary>
    [Range(0, int.MaxValue)] public int WellsCompleted { get; set; }

    /// <summary>Активных нетрадиционных скважин (pozos_no_conv_activos).</summary>
    [Range(0, int.MaxValue)] public int ActiveUnconventionalWells { get; set; }

    /// <summary>Средняя глубина бурения на одну законченную скважину, м.</summary>
    public double MetersPerWell
    {
        get
        {
            if (WellsCompleted == 0)
                return 0;
            return Math.Round(MetersDrilled / WellsCompleted, 1);
        }
    }

    /// <summary>Категория цены Brent: Low (меньше 50), Medium (50-80), High (80 и выше).</summary>
    public string PriceCategory
    {
        get
        {
            if (BrentPrice < 50)
                return "Low";
            if (BrentPrice < 80)
                return "Medium";
            return "High";
        }
    }
}
