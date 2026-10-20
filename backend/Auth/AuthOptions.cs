using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace OilGasApi.Auth;

/// <summary>Настройки JWT-токена и его выдача. Те же значения используются при проверке токена в Program.cs.</summary>
public static class AuthOptions
{
    public const string Issuer = "OilGasApi";      // кто выдал токен
    public const string Audience = "OilGasClient"; // для кого токен
    public const int LifetimeInHours = 12;

    // Секретный ключ для подписи. Для HMAC-SHA256 нужно не меньше 32 символов.
    // В учебном проекте хранится в коде, в настоящем проекте его выносят из репозитория.
    private const string Key = "oilgas-diploma-secret-key-0123456789";

    public static SymmetricSecurityKey SigningKey => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));

    /// <summary>Создать токен, в котором записаны имя пользователя и его роль.</summary>
    public static string GenerateToken(string username, string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimsIdentity.DefaultNameClaimType, username),
            new Claim(ClaimsIdentity.DefaultRoleClaimType, role)
        };

        var jwt = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(LifetimeInHours),
            signingCredentials: new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}
