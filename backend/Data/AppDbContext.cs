using Microsoft.EntityFrameworkCore;
using OilGasApi.Models;

namespace OilGasApi.Data;

public class AppDbContext : DbContext
{
    // каждая строка ниже это таблица в базе
    public DbSet<Basin> Basins => Set<Basin>();
    public DbSet<Production> Productions => Set<Production>();
    public DbSet<BasinProduction> BasinProductions => Set<BasinProduction>();
    public DbSet<DrillingActivity> DrillingActivities => Set<DrillingActivity>();
    public DbSet<MarketFactor> MarketFactors => Set<MarketFactor>();
    public DbSet<MarketEvent> MarketEvents => Set<MarketEvent>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // за один месяц может быть только одна запись
        modelBuilder.Entity<Production>().HasIndex(p => p.Date).IsUnique();
        modelBuilder.Entity<DrillingActivity>().HasIndex(d => d.Date).IsUnique();
        modelBuilder.Entity<MarketFactor>().HasIndex(m => m.Date).IsUnique();

        // у бассейна одна запись за месяц
        modelBuilder.Entity<BasinProduction>().HasIndex(b => new { b.Date, b.BasinId }).IsUnique();
        modelBuilder.Entity<Basin>().HasIndex(b => b.Code).IsUnique();
    }
}
