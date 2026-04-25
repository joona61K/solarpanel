using System.Text.Json;
using Hems.Api.Data;
using Hems.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Hems.Api.Services;

public class SpotPriceService : ISpotPriceService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SpotPriceService> _logger;
    private readonly HemsOptions _options;
    private readonly Random _random = new();

    public SpotPriceService(
        IHttpClientFactory httpClientFactory,
        IServiceScopeFactory scopeFactory,
        ILogger<SpotPriceService> logger,
        IOptions<HemsOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<List<SpotPrice>> GetTodaysPricesAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("SpotPriceService");
            client.Timeout = TimeSpan.FromSeconds(10);
            var response = await client.GetAsync("https://api.spot-hinta.fi/TodayAndDayForward");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var items = JsonSerializer.Deserialize<List<SpotHintaItem>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (items == null || items.Count == 0)
                return await GetSimulatedPricesAsync();

            var today = DateTime.UtcNow.Date;
            var prices = items
                .Where(i => i.HourUTC.Date == today)
                .Select(i => new SpotPrice
                {
                    HourUtc = i.HourUTC,
                    Currency = "EUR",
                    PricePerKwh = i.PriceWithTax / 100.0,
                    Area = _options.SpotPriceArea
                })
                .ToList();

            await SavePricesAsync(prices);
            return prices;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Spot price API unreachable ({Message}), using simulated data", ex.Message);
            return await GetSimulatedPricesAsync();
        }
    }

    private async Task<List<SpotPrice>> GetSimulatedPricesAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HemsDbContext>();
        var today = DateTime.UtcNow.Date;
        var existing = await db.SpotPrices
            .Where(p => p.HourUtc >= today && p.HourUtc < today.AddDays(1))
            .ToListAsync();

        if (existing.Count == 24)
            return existing;

        var basePrices = new double[]
        {
            0.04, 0.03, 0.03, 0.04, 0.05, 0.07,
            0.10, 0.14, 0.13, 0.11, 0.09, 0.08,
            0.09, 0.10, 0.11, 0.13, 0.16, 0.18,
            0.20, 0.17, 0.14, 0.11, 0.08, 0.05
        };

        var prices = Enumerable.Range(0, 24).Select(h => new SpotPrice
        {
            HourUtc = today.AddHours(h),
            Currency = "EUR",
            PricePerKwh = Math.Round(basePrices[h] + (_random.NextDouble() - 0.5) * 0.02, 4),
            Area = _options.SpotPriceArea
        }).ToList();

        await SavePricesAsync(prices);
        return prices;
    }

    private async Task SavePricesAsync(List<SpotPrice> prices)
    {
        if (prices.Count == 0) return;
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HemsDbContext>();
        var minHour = prices.Min(p => p.HourUtc);
        var maxHour = prices.Max(p => p.HourUtc);
        var existingKeys = await db.SpotPrices
            .Where(p => p.HourUtc >= minHour && p.HourUtc <= maxHour)
            .Select(p => new { p.HourUtc, p.Area })
            .ToListAsync();
        var existingSet = existingKeys.Select(k => (k.HourUtc, k.Area)).ToHashSet();
        foreach (var price in prices)
        {
            if (!existingSet.Contains((price.HourUtc, price.Area)))
                db.SpotPrices.Add(price);
        }
        await db.SaveChangesAsync();
    }

    private class SpotHintaItem
    {
        public DateTime HourUTC { get; set; }
        public double PriceWithTax { get; set; }
        public double PriceNoTax { get; set; }
    }
}
