using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OilGasApi.Auth;
using OilGasApi.Data;

var builder = WebApplication.CreateBuilder(args);

// подключаем базу sqlite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// проверяем токен по тем же данным что при его выдаче
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

// разрешаем запросы с сайта на angular для лабы 3
builder.Services.AddCors(options => options.AddPolicy("angular", policy =>
    policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// при первом запуске создаём базу и загружаем данные из папки Data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var dataFolder = Path.Combine(AppContext.BaseDirectory, "Data");
    DataSeeder.Seed(db, dataFolder);
}

app.UseCors("angular");
app.UseAuthentication(); // кто пришёл
app.UseAuthorization();  // можно ли ему сюда
app.MapControllers();

app.Run();





//«Разработка автоматизированной системы прогнозирования и геовизуализации показателей добычи нефти и газа (на основе данных нефтегазовых бассейнов Аргентины)»