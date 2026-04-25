using Microsoft.EntityFrameworkCore;
using Hems.Api.Models;

namespace Hems.Api.Data;

public class HemsDbContext : DbContext
{
    public HemsDbContext(DbContextOptions<HemsDbContext> options) : base(options) { }

    public DbSet<EnergyReading> EnergyReadings => Set<EnergyReading>();
    public DbSet<SpotPrice> SpotPrices => Set<SpotPrice>();
    public DbSet<WeatherForecastEntry> WeatherForecasts => Set<WeatherForecastEntry>();
    public DbSet<AutomationLog> AutomationLogs => Set<AutomationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EnergyReading>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Timestamp).IsRequired();
            e.Ignore(x => x.SurplusWatts);
            e.HasIndex(x => x.Timestamp);
        });

        modelBuilder.Entity<SpotPrice>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.HourUtc).IsRequired();
            e.HasIndex(x => new { x.HourUtc, x.Area });
        });

        modelBuilder.Entity<WeatherForecastEntry>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ForecastTime).IsRequired();
            e.HasIndex(x => x.ForecastTime);
        });

        modelBuilder.Entity<AutomationLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Timestamp).IsRequired();
            e.HasIndex(x => x.Timestamp);
        });
    }
}
