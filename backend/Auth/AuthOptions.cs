using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace OilGasApi.Auth;

public static class AuthOptions
{
    public const string Issuer = "OilGasApi";
    public const string Audience = "OilGasClient";
    public const int LifetimeInHours = 12;

    // ключ для подписи токена, не короче 32 символов
    private const string Key = "oilgas-diploma-secret-key-0123456789";

    public static SymmetricSecurityKey SigningKey => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));

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
