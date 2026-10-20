using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OilGasApi.Auth;
using OilGasApi.Data;

var builder = WebApplication.CreateBuilder(args);

// База данных SQLite: файл oilgas.db рядом с приложением
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Проверка JWT-токена: те же Issuer, Audience и ключ, что при выдаче в AuthOptions
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = AuthOptions.Issuer,

        ValidateAudience = true,
        ValidAudience = AuthOptions.Audience,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = AuthOptions.SigningKey,

        ValidateLifetime = true,
    };
});

builder.Services.AddControllers();

// Разрешаем запросы из Angular-клиента (лаба 3), он работает на порту 4200
builder.Services.AddCors(options => options.AddPolicy("angular", policy =>
    policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// При запуске создаём БД и заполняем её из CSV, если она пустая
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var csvPath = Path.Combine(AppContext.BaseDirectory, "Data", "argentina_oil_gas.csv");
    DataSeeder.Seed(db, csvPath);
}

app.UseCors("angular");
app.UseAuthentication(); // кто пришёл: читает токен
app.UseAuthorization();  // можно ли ему сюда: проверяет [Authorize]
app.MapControllers();

app.Run();
