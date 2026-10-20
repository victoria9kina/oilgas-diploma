namespace OilGasApi.Models;

/// <summary>
/// Добыча по одному бассейну за один месяц.
/// Получается "разворотом" (melt) колонок cuenca_* датасета: одна строка CSV -> 5 записей.
/// </summary>
public class BasinProduction
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }

    /// <summary>Код бассейна: neuquina, gsj, austral, noroeste, cuyana.</summary>
    public string Basin { get; set; } = "";

    public double Value { get; set; }

    // ---------- Бизнес-логика ----------

    /// <summary>Справочная информация о бассейне (название, координаты для карты).</summary>
    public BasinInfo? Info() => Basins.Find(Basin);

    /// <summary>Доля бассейна в переданном итоге (например, в сумме по всем бассейнам), %.</summary>
    public double SharePercentOf(double total) =>
        total > 0 ? Math.Round(Value / total * 100, 2) : 0;

    /// <summary>Изменение относительно предыдущего значения того же бассейна, %.</summary>
    public double ChangePercentFrom(BasinProduction previous)
    {
        if (previous.Basin != Basin)
            throw new InvalidOperationException("Сравнивать можно только записи одного бассейна");
        return previous.Value > 0 ? Math.Round((Value - previous.Value) / previous.Value * 100, 2) : 0;
    }

    /// <summary>Является ли бассейн "лидером" (доля больше порога).</summary>
    public bool IsDominant(double total, double thresholdPercent = 50) =>
        SharePercentOf(total) >= thresholdPercent;
}
