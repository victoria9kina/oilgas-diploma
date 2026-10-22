using System.ComponentModel.DataAnnotations;

namespace OilGasApi.Models;

public class BasinProduction
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    [Required] public string Basin { get; set; } = "";

    [Range(0, double.MaxValue)] public double Value { get; set; }

    // название бассейна по его коду
    public string? BasinName => Basins.Find(Basin)?.Name;

    // доля бассейна от общей суммы, %
    public double SharePercentOf(double total) => Percent.Of(Value, total);
}
