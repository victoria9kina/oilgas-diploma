using System.ComponentModel.DataAnnotations;

namespace OilGasApi.Models;

/// <summary>Национальная добыча нефти и газа за один месяц (колонки petroleo_* и gas_* датасета).</summary>
public class Production
{
    public int Id { get; set; }

    /// <summary>Месяц (первое число месяца).</summary>
    public DateOnly Date { get; set; }

    [Range(0, double.MaxValue)] public double OilConventional { get; set; }
    [Range(0, double.MaxValue)] public double OilShale { get; set; }
    [Range(0, double.MaxValue)] public double OilTight { get; set; }
    public double OilTotal { get; set; }

    [Range(0, double.MaxValue)] public double GasConventional { get; set; }
    [Range(0, double.MaxValue)] public double GasShale { get; set; }
    [Range(0, double.MaxValue)] public double GasTight { get; set; }
    public double GasTotal { get; set; }

    // Вычисляемые свойства: не хранятся в БД (нет set), но попадают в JSON-ответ

    /// <summary>Нетрадиционная нефть = сланцевая + tight (из плотных пород).</summary>
    public double OilUnconventional => OilShale + OilTight;

    /// <summary>Нетрадиционный газ = сланцевый + tight.</summary>
    public double GasUnconventional => GasShale + GasTight;

    /// <summary>Доля нетрадиционной нефти в общей добыче, %.</summary>
    public double OilUnconventionalSharePercent => Percent.Of(OilUnconventional, OilTotal);

    /// <summary>Доля нетрадиционного газа в общей добыче, %.</summary>
    public double GasUnconventionalSharePercent => Percent.Of(GasUnconventional, GasTotal);

    /// <summary>Пересчитать итоги как сумму видов добычи. Вызывается при создании и изменении записи,
    /// чтобы итог не приходил с клиента и всегда совпадал с суммой.</summary>
    public void RecalculateTotals()
    {
        OilTotal = OilConventional + OilShale + OilTight;
        GasTotal = GasConventional + GasShale + GasTight;
    }
}
