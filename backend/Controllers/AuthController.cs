using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OilGasApi.Auth;
using OilGasApi.Models;

namespace OilGasApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    // Учебные пользователи. Пароли открытым текстом допустимы только в лабораторной работе.
    private static readonly List<User> Users = new List<User>
    {
        new User { Username = "admin", Password = "admin123", Role = "admin" },
        new User { Username = "user", Password = "user123", Role = "user" }
    };

    public class LoginData
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    // POST api/auth/login  { "username": "admin", "password": "admin123" }
    [HttpPost("login")]
    public IActionResult Login(LoginData loginData)
    {
        var user = Users.FirstOrDefault(u => u.Username == loginData.Username && u.Password == loginData.Password);
        if (user == null)
            return Unauthorized(new { message = "Неверный логин или пароль" });

        var token = AuthOptions.GenerateToken(user.Username, user.Role);
        return Ok(new { token, username = user.Username, role = user.Role });
    }

    // GET api/auth/me: кто я. Удобно проверить, что токен работает.
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var username = User.Identity?.Name;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        return Ok(new { username, role });
    }
}
