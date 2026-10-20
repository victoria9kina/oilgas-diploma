namespace OilGasApi.Models;

/// <summary>Расчёт процентов: одна формула для всех моделей и отчётов.</summary>
public static class Percent
{
    /// <summary>Какой процент составляет part от total. Если total = 0, возвращает 0 (делить на ноль нельзя).</summary>
    public static double Of(double part, double total)
    {
        if (total == 0)
            return 0;
        return Math.Round(part / total * 100, 2);
    }
}
