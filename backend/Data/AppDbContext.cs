using Microsoft.EntityFrameworkCore;
using OilGasApi.Models;

namespace OilGasApi.Data;

/// <summary>Связь с базой данных: каждый DbSet это таблица.</summary>
public class AppDbContext : DbContext
{
    // Set<T>() возвращает таблицу для класса T. Запись через "=>" нужна, чтобы компилятор
    // не предупреждал, что свойство может быть null (таблицы создаёт сам EF).
    public DbSet<Production> Productions => Set<Production>();
    public DbSet<BasinProduction> BasinProductions => Set<BasinProduction>();
    public DbSet<DrillingActivity> DrillingActivities => Set<DrillingActivity>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Один месяц = одна запись национальной добычи и одна запись бурения
        modelBuilder.Entity<Production>().HasIndex(p => p.Date).IsUnique();
        modelBuilder.Entity<DrillingActivity>().HasIndex(d => d.Date).IsUnique();

        // Для бассейна уникальна пара (месяц, бассейн)
        modelBuilder.Entity<BasinProduction>().HasIndex(b => new { b.Date, b.Basin }).IsUnique();
    }
}
