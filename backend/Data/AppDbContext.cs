using Microsoft.EntityFrameworkCore;
using OilGasApi.Models;

namespace OilGasApi.Data;

public class AppDbContext : DbContext
{
    public DbSet<Production> Productions => Set<Production>();
    public DbSet<BasinProduction> BasinProductions => Set<BasinProduction>();
    public DbSet<DrillingActivity> DrillingActivities => Set<DrillingActivity>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // за один месяц может быть только одна запись
        modelBuilder.Entity<Production>().HasIndex(p => p.Date).IsUnique();
        modelBuilder.Entity<DrillingActivity>().HasIndex(d => d.Date).IsUnique();

        // у бассейна одна запись за месяц
        modelBuilder.Entity<BasinProduction>().HasIndex(b => new { b.Date, b.Basin }).IsUnique();
    }
}
