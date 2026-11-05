using System.ComponentModel.DataAnnotations;

namespace OilGasApi.Models;

// важное событие на рынке нефти вместо потока новостей
public class MarketEvent
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    [Required] public string Title { get; set; } = "";

    [Required] public string Category { get; set; } = "";

    // влияние на добычу -1 падение 0 неясно 1 рост
    [Range(-1, 1)] public int Impact { get; set; }

    public string SourceUrl { get; set; } = "";

    // влияние словами чтобы было понятно в отчёте
    public string ImpactName
    {
        get
        {
            if (Impact > 0)
                return "рост";
            if (Impact < 0)
                return "падение";
            return "неясно";
        }
    }
}
