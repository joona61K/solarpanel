using Hems.Api.Data;
using Hems.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hems.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AutomationController : ControllerBase
{
    private readonly HemsDbContext _db;
    private readonly AutomationEngineService _automationEngine;
    private readonly IConfiguration _config;

    public AutomationController(HemsDbContext db, AutomationEngineService automationEngine, IConfiguration config)
    {
        _db = db;
        _automationEngine = automationEngine;
        _config = config;
    }

    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs([FromQuery] int limit = 50)
    {
        if (limit < 1 || limit > 500) limit = 50;
        var logs = await _db.AutomationLogs
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .ToListAsync();
        return Ok(logs);
    }

    [HttpGet("rules")]
    public IActionResult GetRules()
    {
        var surplus = _config.GetValue<double>("Hems:SurplusThresholdWatts", 500.0);
        var cheap = _config.GetValue<double>("Hems:CheapPriceThresholdEur", 0.05);
        var expensive = _config.GetValue<double>("Hems:ExpensivePriceThresholdEur", 0.20);

        var rules = new[]
        {
            new
            {
                id = 1,
                name = "Solar Surplus → Water Heater ON",
                description = $"If production surplus > {surplus}W, turn on WaterHeater relay",
                condition = $"SurplusWatts > {surplus}",
                action = "TurnOn WaterHeater",
                enabled = true
            },
            new
            {
                id = 2,
                name = "Cheap Price → Car Charger ON",
                description = $"If spot price < {cheap} EUR/kWh, turn on CarCharger relay",
                condition = $"SpotPrice < {cheap}",
                action = "TurnOn CarCharger",
                enabled = true
            },
            new
            {
                id = 3,
                name = "Expensive Price → All OFF",
                description = $"If spot price > {expensive} EUR/kWh, turn off all non-essential relays",
                condition = $"SpotPrice > {expensive}",
                action = "TurnOff WaterHeater, TurnOff CarCharger",
                enabled = true
            }
        };
        return Ok(rules);
    }

    [HttpPost("trigger")]
    public async Task<IActionResult> Trigger()
    {
        await _automationEngine.EvaluateRulesAsync();
        return Ok(new { message = "Automation evaluation triggered", timestamp = DateTime.UtcNow });
    }
}
