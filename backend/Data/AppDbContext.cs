using Microsoft.EntityFrameworkCore;
using OilGasApi.Models;

namespace OilGasApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Production> Productions => Set<Production>();
    public DbSet<BasinProduction> BasinProductions => Set<BasinProduction>();
    public DbSet<DrillingActivity> DrillingActivities => Set<DrillingActivity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Один месяц = одна запись национальной добычи и одна запись бурения
        modelBuilder.Entity<Production>().HasIndex(p => p.Date).IsUnique();
        modelBuilder.Entity<DrillingActivity>().HasIndex(d => d.Date).IsUnique();

        // Для бассейна уникальна пара (месяц, бассейн)
        modelBuilder.Entity<BasinProduction>().HasIndex(b => new { b.Date, b.Basin }).IsUnique();
        modelBuilder.Entity<BasinProduction>().Property(b => b.Basin).HasMaxLength(20);
    }
}
