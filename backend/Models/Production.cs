namespace OilGasApi.Models;

/// <summary>
/// Национальная добыча нефти и газа за один месяц (колонки petroleo_* и gas_* датасета).
/// Единицы измерения те же, что в исходном датасете.
/// </summary>
public class Production
{
    public int Id { get; set; }

    /// <summary>Месяц (всегда первое число месяца).</summary>
    public DateOnly Date { get; set; }

    public double OilConventional { get; set; }
    public double OilShale { get; set; }
    public double OilTight { get; set; }
    public double OilTotal { get; set; }

    public double GasConventional { get; set; }
    public double GasShale { get; set; }
    public double GasTight { get; set; }
    public double GasTotal { get; set; }

    // ---------- Бизнес-логика ----------

    /// <summary>Нетрадиционная нефть = сланцевая + трудноизвлекаемая (tight).</summary>
    public double OilUnconventional() => OilShale + OilTight;

    /// <summary>Нетрадиционный газ = сланцевый + tight.</summary>
    public double GasUnconventional() => GasShale + GasTight;

    /// <summary>Доля нетрадиционной нефти в общей добыче, %.</summary>
    public double OilUnconventionalSharePercent() =>
        OilTotal > 0 ? Math.Round(OilUnconventional() / OilTotal * 100, 2) : 0;

    /// <summary>Доля нетрадиционного газа в общей добыче, %.</summary>
    public double GasUnconventionalSharePercent() =>
        GasTotal > 0 ? Math.Round(GasUnconventional() / GasTotal * 100, 2) : 0;

    /// <summary>Пересчитать итоговые значения как сумму компонент.</summary>
    public void RecalculateTotals()
    {
        OilTotal = OilConventional + OilShale + OilTight;
        GasTotal = GasConventional + GasShale + GasTight;
    }

    /// <summary>
    /// Проверка согласованности: итог совпадает с суммой компонент
    /// с допустимым относительным отклонением (по умолчанию 1%).
    /// </summary>
    public bool IsConsistent(double tolerance = 0.01)
    {
        bool Close(double total, double sum) =>
            total == 0 ? sum == 0 : Math.Abs(total - sum) / total <= tolerance;

        return Close(OilTotal, OilConventional + OilShale + OilTight)
            && Close(GasTotal, GasConventional + GasShale + GasTight);
    }

    /// <summary>Прирост добычи нефти относительно другого месяца, %.</summary>
    public double OilGrowthPercentFrom(Production previous) =>
        previous.OilTotal > 0 ? Math.Round((OilTotal - previous.OilTotal) / previous.OilTotal * 100, 2) : 0;

    /// <summary>Прирост добычи газа относительно другого месяца, %.</summary>
    public double GasGrowthPercentFrom(Production previous) =>
        previous.GasTotal > 0 ? Math.Round((GasTotal - previous.GasTotal) / previous.GasTotal * 100, 2) : 0;
}
