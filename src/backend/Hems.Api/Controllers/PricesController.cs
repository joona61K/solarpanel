using Hems.Api.Data;
using Hems.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hems.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PricesController : ControllerBase
{
    private readonly HemsDbContext _db;
    private readonly ISpotPriceService _spotPriceService;

    public PricesController(HemsDbContext db, ISpotPriceService spotPriceService)
    {
        _db = db;
        _spotPriceService = spotPriceService;
    }

    [HttpGet("today")]
    public async Task<IActionResult> GetToday()
    {
        var prices = await _spotPriceService.GetTodaysPricesAsync();
        return Ok(prices.OrderBy(p => p.HourUtc));
    }

    [HttpGet("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var prices = await _spotPriceService.GetTodaysPricesAsync();
        return Ok(new { message = "Prices refreshed", count = prices.Count });
    }
}
