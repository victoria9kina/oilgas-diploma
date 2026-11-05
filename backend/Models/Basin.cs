using System.ComponentModel.DataAnnotations;

namespace OilGasApi.Models;

public class Basin
{
    public int Id { get; set; }

    // код как в названии колонки csv cuenca_код
    [Required] public string Code { get; set; } = "";

    [Required] public string Name { get; set; } = "";

    // координаты нужны для карты в дипломе
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
