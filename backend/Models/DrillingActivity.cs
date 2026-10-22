using System.ComponentModel.DataAnnotations;

namespace OilGasApi.Models;

public class DrillingActivity
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    [Range(0, 500)] public double BrentPrice { get; set; }

    [Range(0, double.MaxValue)] public double MetersDrilled { get; set; }

    [Range(0, int.MaxValue)] public int WellsCompleted { get; set; }

    [Range(0, int.MaxValue)] public int ActiveUnconventionalWells { get; set; }

    // сколько метров бурения в среднем на одну скважину
    public double MetersPerWell
    {
        get
        {
            if (WellsCompleted == 0)
                return 0; // на ноль делить нельзя
            return Math.Round(MetersDrilled / WellsCompleted, 1);
        }
    }

    // категория цены нефти: low до 50 $, medium до 80 $, high от 80 $
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
