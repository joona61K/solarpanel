using Hems.Api.Data;
using Hems.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Hems.Api.Services;

public class AutomationEngineService : IHostedService, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AutomationEngineService> _logger;
    private readonly HemsOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private Timer? _timer;

    public AutomationEngineService(
        IServiceScopeFactory scopeFactory,
        ILogger<AutomationEngineService> logger,
        IOptions<HemsOptions> options,
        IHttpClientFactory httpClientFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Automation engine starting");
        _timer = new Timer(RunAutomationCallback, null, TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(5));
        return Task.CompletedTask;
    }

    private async void RunAutomationCallback(object? state)
    {
        try
        {
            await EvaluateRulesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in automation engine");
        }
    }

    public async Task EvaluateRulesAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HemsDbContext>();
        var spotService = scope.ServiceProvider.GetRequiredService<ISpotPriceService>();

        var latestReading = await db.EnergyReadings
            .OrderByDescending(r => r.Timestamp)
            .FirstOrDefaultAsync();

        var currentHour = DateTime.UtcNow;
        var spotPrices = await spotService.GetTodaysPricesAsync();
        var currentPrice = spotPrices
            .Where(p => p.HourUtc <= currentHour)
            .OrderByDescending(p => p.HourUtc)
            .FirstOrDefault();

        if (latestReading != null)
        {
            var surplus = latestReading.SurplusWatts;
            if (surplus > _options.SurplusThresholdWatts)
            {
                var success = await ControlRelay("WaterHeater", true);
                await LogActionAsync(db,
                    $"Surplus {surplus:F0}W > threshold {_options.SurplusThresholdWatts}W",
                    "WaterHeater", "TurnOn", success);
            }
        }

        if (currentPrice != null)
        {
            if (currentPrice.PricePerKwh < _options.CheapPriceThresholdEur)
            {
                var success = await ControlRelay("CarCharger", true);
                await LogActionAsync(db,
                    $"Cheap price {currentPrice.PricePerKwh:F4} EUR/kWh < {_options.CheapPriceThresholdEur}",
                    "CarCharger", "TurnOn", success);
            }
            else if (currentPrice.PricePerKwh > _options.ExpensivePriceThresholdEur)
            {
                foreach (var device in new[] { "WaterHeater", "CarCharger" })
                {
                    var success = await ControlRelay(device, false);
                    await LogActionAsync(db,
                        $"Expensive price {currentPrice.PricePerKwh:F4} EUR/kWh > {_options.ExpensivePriceThresholdEur}",
                        device, "TurnOff", success);
                }
            }
        }

        await db.SaveChangesAsync();
    }

    private async Task<bool> ControlRelay(string deviceName, bool turnOn)
    {
        try
        {
            var action = turnOn ? "on" : "off";
            var url = $"{_options.ShellyBaseUrl}/relay/0?turn={action}";
            _logger.LogInformation("Shelly control: {Device} -> {Action} via {Url}", deviceName, action, url);
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var response = await client.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Could not control relay {Device}: {Message}", deviceName, ex.Message);
            return false;
        }
    }

    private async Task LogActionAsync(HemsDbContext db, string reason, string device, string action, bool success)
    {
        var log = new AutomationLog
        {
            Timestamp = DateTime.UtcNow,
            TriggerReason = reason,
            DeviceName = device,
            Action = action,
            Success = success
        };
        db.AutomationLogs.Add(log);
        _logger.LogInformation("Automation: {Device} {Action} ({Reason}) - {Result}",
            device, action, reason, success ? "OK" : "FAILED");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Automation engine stopping");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose() => _timer?.Dispose();
}
