namespace OilGasApi.Models;

public static class Percent
{
    public static double Of(double part, double total)
    {
        if (total == 0)
            return 0; // на ноль делить нельзя
        return Math.Round(part / total * 100, 2);
    }
}
