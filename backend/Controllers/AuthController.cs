using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OilGasApi.Auth;
using OilGasApi.Models;

//проверка пользователя 

namespace OilGasApi.Controllers;

[ApiController]
[Route("api/auth")] //адрес. вход, выдаёт токен
public class AuthController : ControllerBase
{
    // учебные пользователи
    private static readonly List<User> Users = new List<User>
    {
        new User { Username = "admin", Password = "admin123", Role = "admin" },
        new User { Username = "user", Password = "user123", Role = "user" }
    };

    // логин и пароль которые приходят из postman
    public class LoginData
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    [HttpPost("login")]
    public IActionResult Login(LoginData loginData)
    {
        // ищем пользователя с таким логином и паролем
        var user = Users.FirstOrDefault(u => u.Username == loginData.Username && u.Password == loginData.Password);
        if (user == null)
            return Unauthorized(new { message = "Неверный логин или пароль" });

        var token = AuthOptions.GenerateToken(user.Username, user.Role);
        return Ok(new { token, username = user.Username, role = user.Role });
    }

    // показывает кто вошёл по токену
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var username = User.Identity?.Name;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        return Ok(new { username, role });
    }
}
