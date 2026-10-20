namespace OilGasApi.Models;

/// <summary>Буровая активность и цена нефти за месяц.</summary>
public class DrillingActivity
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }

    /// <summary>Цена Brent, USD/баррель (precio_brent).</summary>
    public double? BrentPrice { get; set; }

    /// <summary>Пробурено метров (metros_perforados).</summary>
    public double? MetersDrilled { get; set; }

    /// <summary>Законченных скважин (pozos_terminados).</summary>
    public int? WellsCompleted { get; set; }

    /// <summary>Активных нетрадиционных скважин (pozos_no_conv_activos).</summary>
    public int? ActiveUnconventionalWells { get; set; }

    // ---------- Бизнес-логика ----------

    /// <summary>Средняя проходка на одну законченную скважину, м.</summary>
    public double? MetersPerWell() =>
        MetersDrilled is not null && WellsCompleted is > 0
            ? Math.Round(MetersDrilled.Value / WellsCompleted.Value, 1)
            : null;

    /// <summary>Категория цены: Low (&lt;50), Medium (50–80), High (&gt;=80), Unknown.</summary>
    public string PriceCategory() => BrentPrice switch
    {
        null => "Unknown",
        < 50 => "Low",
        < 80 => "Medium",
        _ => "High"
    };

    /// <summary>Есть ли в месяце буровая активность.</summary>
    public bool HasDrilling() => WellsCompleted is > 0 || MetersDrilled is > 0;
}
