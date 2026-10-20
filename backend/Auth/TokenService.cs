using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace OilGasApi.Auth;

public class JwtOptions
{
    public string Issuer { get; set; } = "OilGasApi";
    public string Audience { get; set; } = "OilGasClient";
    public string Key { get; set; } = "";
    public int LifetimeMinutes { get; set; } = 120;

    public SymmetricSecurityKey SigningKey() => new(Encoding.UTF8.GetBytes(Key));
}

/// <summary>Демо-пользователь из appsettings.json. Для лабы пароли хранятся открыто, в реальном проекте так нельзя.</summary>
public class DemoUser
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = Roles.User;
}

/// <summary>Короткие имена claim'ов в токене (одинаковые при выдаче и проверке).</summary>
public static class ClaimNames
{
    public const string Name = "name";
    public const string Role = "role";
}

public static class Roles
{
    public const string Admin = "admin";
    public const string User = "user";
}

public class TokenService(JwtOptions options)
{
    public (string Token, DateTime ExpiresAt) Create(string username, string role)
    {
        var expires = DateTime.UtcNow.AddMinutes(options.LifetimeMinutes);
        var claims = new[]
        {
            new Claim(ClaimNames.Name, username),
            new Claim(ClaimNames.Role, role),
        };
        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: new SigningCredentials(options.SigningKey(), SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
