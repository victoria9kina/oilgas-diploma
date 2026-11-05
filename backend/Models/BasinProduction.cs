using System.ComponentModel.DataAnnotations;

namespace OilGasApi.Models;

public class BasinProduction
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    // внешний ключ на таблицу бассейнов
    public int BasinId { get; set; }

    // сам бассейн подгружается через Include
    public Basin? Basin { get; set; }

    [Range(0, double.MaxValue)] public double Value { get; set; }

    // доля бассейна от общей суммы в процентах
    public double SharePercentOf(double total) => Percent.Of(Value, total);
}
