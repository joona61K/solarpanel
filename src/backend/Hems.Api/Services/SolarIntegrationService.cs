using System.Text.Json;
using Hems.Api.Data;
using Hems.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Hems.Api.Services;

public class HemsOptions
{
    public string InverterUrl { get; set; } = "http://192.168.1.100";
    public string SpotPriceArea { get; set; } = "FI";
    public double WeatherLatitude { get; set; } = 60.17;
    public double WeatherLongitude { get; set; } = 24.94;
    public string ShellyBaseUrl { get; set; } = "http://192.168.1.200";
    public double SurplusThresholdWatts { get; set; } = 500.0;
    public double CheapPriceThresholdEur { get; set; } = 0.05;
    public double ExpensivePriceThresholdEur { get; set; } = 0.20;
}

public class SolarIntegrationService : ISolarIntegrationService, IHostedService, IDisposable
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SolarIntegrationService> _logger;
    private readonly HemsOptions _options;
    private Timer? _timer;
    private readonly Random _random = new();
    private EnergyReading? _latestReading;

    public SolarIntegrationService(
        IHttpClientFactory httpClientFactory,
        IServiceScopeFactory scopeFactory,
        ILogger<SolarIntegrationService> logger,
        IOptions<HemsOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<EnergyReading> GetCurrentReadingAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("SolarService");
            client.Timeout = TimeSpan.FromSeconds(5);
            var url = $"{_options.InverterUrl}/solar_api/v1/GetPowerFlowRealtimeData.fcgi";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var body = doc.RootElement.GetProperty("Body").GetProperty("Data");
            var site = body.GetProperty("Site");

            var production = site.TryGetProperty("P_PV", out var pv) && pv.ValueKind != JsonValueKind.Null
                ? pv.GetDouble() : 0;
            var consumption = site.TryGetProperty("P_Load", out var load) && load.ValueKind != JsonValueKind.Null
                ? Math.Abs(load.GetDouble()) : 0;
            var gridFeedIn = site.TryGetProperty("P_Grid", out var grid) && grid.ValueKind != JsonValueKind.Null
                ? -grid.GetDouble() : 0;
            var battery = site.TryGetProperty("P_Akku", out var akku) && akku.ValueKind != JsonValueKind.Null
                ? akku.GetDouble() : 0;

            var reading = new EnergyReading
            {
                Timestamp = DateTime.UtcNow,
                ProductionWatts = production,
                ConsumptionWatts = consumption,
                GridFeedInWatts = gridFeedIn,
                BatteryChargePercent = battery
            };
            _latestReading = reading;
            return reading;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Inverter unreachable ({Message}), using simulated data", ex.Message);
            return GenerateSimulatedReading();
        }
    }

    private EnergyReading GenerateSimulatedReading()
    {
        var hour = DateTime.Now.Hour;
        var solarFactor = hour >= 6 && hour <= 20
            ? Math.Sin((hour - 6) * Math.PI / 14.0)
            : 0;
        var production = solarFactor * (_random.NextDouble() * 3000 + 500);
        var consumption = _random.NextDouble() * 1200 + 800;
        var surplus = production - consumption;

        var reading = new EnergyReading
        {
            Timestamp = DateTime.UtcNow,
            ProductionWatts = Math.Round(production, 1),
            ConsumptionWatts = Math.Round(consumption, 1),
            GridFeedInWatts = Math.Round(Math.Max(0, surplus), 1),
            BatteryChargePercent = Math.Round(_random.NextDouble() * 100, 1)
        };
        _latestReading = reading;
        return reading;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Solar integration service starting");
        _timer = new Timer(SaveReadingCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(60));
        return Task.CompletedTask;
    }

    private async void SaveReadingCallback(object? state)
    {
        try
        {
            var reading = await GetCurrentReadingAsync();
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<HemsDbContext>();
            db.EnergyReadings.Add(reading);
            await db.SaveChangesAsync();
            _logger.LogDebug("Saved energy reading: Production={P}W, Consumption={C}W",
                reading.ProductionWatts, reading.ConsumptionWatts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving energy reading");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Solar integration service stopping");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose() => _timer?.Dispose();
}
