namespace OilGasApi.Models;

/// <summary>Пользователь API. Роль admin может менять данные, роль user только читать отчёты.</summary>
public class User
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "";
}
