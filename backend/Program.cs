using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OilGasApi.Auth;
using OilGasApi.Data;

var builder = WebApplication.CreateBuilder(args);

// ---------- БД: SQLite (файл oilgas.db рядом с приложением) ----------
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=oilgas.db"));

// ---------- JWT-авторизация ----------
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
if (jwt.Key.Length < 32)
    throw new InvalidOperationException("Jwt:Key в appsettings.json должен быть не короче 32 символов");

builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton<TokenService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false; // роли и имя берём из токена как есть
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwt.SigningKey(),
            RoleClaimType = ClaimNames.Role,
            NameClaimType = ClaimNames.Name,
        };

        // Понятные JSON-ответы вместо пустых 401/403
        o.Events = new JwtBearerEvents
        {
            OnChallenge = async ctx =>
            {
                ctx.HandleResponse();
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await ctx.Response.WriteAsJsonAsync(new { status = 401, message = "Нужна авторизация: передайте JWT-токен в заголовке Authorization" });
            },
            OnForbidden = async ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                await ctx.Response.WriteAsJsonAsync(new { status = 403, message = "Недостаточно прав: операция доступна только администратору" });
            }
        };
    });
builder.Services.AddAuthorization();

// ---------- Контроллеры ----------
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

// CORS для будущего Angular-клиента (лаба по клиентской части)
builder.Services.AddCors(o => o.AddPolicy("angular", p =>
    p.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// ---------- Создание БД и импорт CSV при первом запуске ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var csvPath = Path.Combine(AppContext.BaseDirectory,
        builder.Configuration["Dataset:CsvPath"] ?? "Data/argentina_oil_gas.csv");
    await DataSeeder.SeedAsync(db, csvPath, app.Logger);
}

app.UseCors("angular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
