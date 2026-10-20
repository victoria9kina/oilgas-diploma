using System.ComponentModel.DataAnnotations;

namespace OilGasApi.Models;

/// <summary>
/// Добыча нефти в одном бассейне за один месяц.
/// В CSV каждый бассейн это отдельная колонка cuenca_*, здесь каждая колонка стала отдельной записью:
/// одна строка CSV даёт 5 записей BasinProduction.
/// </summary>
public class BasinProduction
{
    public int Id { get; set; }

    /// <summary>Месяц (первое число месяца).</summary>
    public DateOnly Date { get; set; }

    /// <summary>Код бассейна: neuquina, gsj, austral, noroeste, cuyana.</summary>
    [Required] public string Basin { get; set; } = "";

    [Range(0, double.MaxValue)] public double Value { get; set; }

    /// <summary>Название бассейна из справочника.</summary>
    public string? BasinName => Basins.Find(Basin)?.Name;

    /// <summary>Доля бассейна (в %) от переданной суммы, например от добычи всех бассейнов за месяц.</summary>
    public double SharePercentOf(double total) => Percent.Of(Value, total);
}
