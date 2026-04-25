using Hems.Api.Data;
using Hems.Api.Models;
using Hems.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hems.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly HemsDbContext _db;
    private readonly ISolarIntegrationService _solarService;
    private readonly ISpotPriceService _spotPriceService;
    private readonly IWeatherForecastService _weatherService;

    public DashboardController(
        HemsDbContext db,
        ISolarIntegrationService solarService,
        ISpotPriceService spotPriceService,
        IWeatherForecastService weatherService)
    {
        _db = db;
        _solarService = solarService;
        _spotPriceService = spotPriceService;
        _weatherService = weatherService;
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent()
    {
        var reading = await _solarService.GetCurrentReadingAsync();

        var now = DateTime.UtcNow;
        var spotPrices = await _spotPriceService.GetTodaysPricesAsync();
        var currentSpot = spotPrices
            .Where(p => p.HourUtc <= now)
            .OrderByDescending(p => p.HourUtc)
            .FirstOrDefault();

        var weather = await _db.WeatherForecasts
            .Where(w => w.ForecastTime <= now)
            .OrderByDescending(w => w.ForecastTime)
            .FirstOrDefaultAsync();

        if (weather == null)
        {
            var forecasts = await _weatherService.GetForecastAsync();
            weather = forecasts.FirstOrDefault();
        }

        var lastLog = await _db.AutomationLogs
            .OrderByDescending(l => l.Timestamp)
            .FirstOrDefaultAsync();

        return Ok(new
        {
            reading,
            currentSpotPrice = currentSpot,
            weather,
            lastAutomationAction = lastLog
        });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] int days = 7)
    {
        if (days < 1 || days > 90) days = 7;
        var from = DateTime.UtcNow.AddDays(-days);

        var readings = await _db.EnergyReadings
            .Where(r => r.Timestamp >= from)
            .OrderBy(r => r.Timestamp)
            .ToListAsync();

        var hourly = readings
            .GroupBy(r => new DateTime(r.Timestamp.Year, r.Timestamp.Month, r.Timestamp.Day, r.Timestamp.Hour, 0, 0, DateTimeKind.Utc))
            .Select(g => new
            {
                timestamp = g.Key,
                avgProductionWatts = g.Average(r => r.ProductionWatts),
                avgConsumptionWatts = g.Average(r => r.ConsumptionWatts),
                avgGridFeedInWatts = g.Average(r => r.GridFeedInWatts),
                avgBatteryChargePercent = g.Average(r => r.BatteryChargePercent)
            })
            .OrderBy(x => x.timestamp)
            .ToList();

        return Ok(hourly);
    }
}
