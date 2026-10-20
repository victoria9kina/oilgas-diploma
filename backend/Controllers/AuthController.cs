using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OilGasApi.Auth;
using OilGasApi.Dtos;

namespace OilGasApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IConfiguration config, TokenService tokens) : ControllerBase
{
    /// <summary>Вход: возвращает JWT-токен с ролью пользователя.</summary>
    [HttpPost("login")]
    public IActionResult Login(LoginDto dto)
    {
        var users = config.GetSection("DemoUsers").Get<List<DemoUser>>() ?? [];

        var user = users
            .Where(u => u.Username == dto.Username && u.Password == dto.Password)
            .Select(u => new { u.Username, u.Role })
            .FirstOrDefault();

        if (user is null)
            return Unauthorized(new { message = "Неверный логин или пароль" });

        var (token, expiresAt) = tokens.Create(user.Username, user.Role);
        return Ok(new { token, expiresAt, username = user.Username, role = user.Role });
    }

    /// <summary>Кто я: удобно проверять, что токен работает.</summary>
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me() => Ok(new
    {
        username = User.Identity?.Name,
        role = User.IsInRole(Roles.Admin) ? Roles.Admin : Roles.User
    });
}
