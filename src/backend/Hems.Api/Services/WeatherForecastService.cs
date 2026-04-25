using System.Text.Json;
using Hems.Api.Data;
using Hems.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Hems.Api.Services;

public class WeatherForecastService : IWeatherForecastService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WeatherForecastService> _logger;
    private readonly HemsOptions _options;
    private readonly Random _random = new();

    public WeatherForecastService(
        IHttpClientFactory httpClientFactory,
        IServiceScopeFactory scopeFactory,
        ILogger<WeatherForecastService> logger,
        IOptions<HemsOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<List<WeatherForecastEntry>> GetForecastAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("WeatherService");
            client.Timeout = TimeSpan.FromSeconds(10);
            var lat = _options.WeatherLatitude;
            var lon = _options.WeatherLongitude;
            var url = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}" +
                      "&hourly=temperature_2m,cloudcover,shortwave_radiation,precipitation&forecast_days=2";

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var hourly = doc.RootElement.GetProperty("hourly");

            var times = hourly.GetProperty("time").EnumerateArray().Select(t => DateTime.Parse(t.GetString()!)).ToList();
            var temps = hourly.GetProperty("temperature_2m").EnumerateArray().Select(t => t.GetDouble()).ToList();
            var clouds = hourly.GetProperty("cloudcover").EnumerateArray().Select(t => t.GetDouble()).ToList();
            var radiation = hourly.GetProperty("shortwave_radiation").EnumerateArray().Select(t => t.GetDouble()).ToList();
            var precip = hourly.GetProperty("precipitation").EnumerateArray().Select(t => t.GetDouble()).ToList();

            var entries = times.Select((t, i) => new WeatherForecastEntry
            {
                ForecastTime = DateTime.SpecifyKind(t, DateTimeKind.Utc),
                TemperatureCelsius = temps[i],
                CloudCoverPercent = clouds[i],
                SolarRadiationWm2 = radiation[i],
                PrecipitationMm = precip[i]
            }).ToList();

            await SaveForecastAsync(entries);
            return entries;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Weather API unreachable ({Message}), using simulated data", ex.Message);
            return GenerateSimulatedForecast();
        }
    }

    private List<WeatherForecastEntry> GenerateSimulatedForecast()
    {
        var now = DateTime.UtcNow;
        return Enumerable.Range(0, 48).Select(h =>
        {
            var t = now.Date.AddHours(h);
            var hour = t.Hour;
            var solarFactor = hour >= 6 && hour <= 20
                ? Math.Sin((hour - 6) * Math.PI / 14.0) : 0;
            return new WeatherForecastEntry
            {
                ForecastTime = t,
                TemperatureCelsius = Math.Round(15 + _random.NextDouble() * 10 - 5, 1),
                CloudCoverPercent = Math.Round(_random.NextDouble() * 100, 1),
                SolarRadiationWm2 = Math.Round(solarFactor * (600 + _random.NextDouble() * 200), 1),
                PrecipitationMm = Math.Round(_random.NextDouble() * 2, 2)
            };
        }).ToList();
    }

    private async Task SaveForecastAsync(List<WeatherForecastEntry> entries)
    {
        if (entries.Count == 0) return;
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HemsDbContext>();
        var cutoff = DateTime.UtcNow.AddDays(-1);
        var old = db.WeatherForecasts.Where(w => w.ForecastTime < cutoff);
        db.WeatherForecasts.RemoveRange(old);
        var minTime = entries.Min(e => e.ForecastTime);
        var maxTime = entries.Max(e => e.ForecastTime);
        var existingTimes = (await db.WeatherForecasts
            .Where(w => w.ForecastTime >= minTime && w.ForecastTime <= maxTime)
            .Select(w => w.ForecastTime)
            .ToListAsync()).ToHashSet();
        foreach (var entry in entries)
        {
            if (!existingTimes.Contains(entry.ForecastTime))
                db.WeatherForecasts.Add(entry);
        }
        await db.SaveChangesAsync();
    }
}
