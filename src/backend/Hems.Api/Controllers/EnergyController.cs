using Hems.Api.Data;
using Hems.Api.Models;
using Hems.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hems.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnergyController : ControllerBase
{
    private readonly HemsDbContext _db;
    private readonly ISolarIntegrationService _solarService;

    public EnergyController(HemsDbContext db, ISolarIntegrationService solarService)
    {
        _db = db;
        _solarService = solarService;
    }

    [HttpGet("readings")]
    public async Task<IActionResult> GetReadings(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 500) pageSize = 50;

        var query = _db.EnergyReadings.AsQueryable();
        if (from.HasValue) query = query.Where(r => r.Timestamp >= from.Value.ToUniversalTime());
        if (to.HasValue) query = query.Where(r => r.Timestamp <= to.Value.ToUniversalTime());

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }

    [HttpGet("readings/latest")]
    public async Task<IActionResult> GetLatest()
    {
        var reading = await _db.EnergyReadings
            .OrderByDescending(r => r.Timestamp)
            .FirstOrDefaultAsync();

        if (reading == null)
            reading = await _solarService.GetCurrentReadingAsync();

        return Ok(reading);
    }

    [HttpPost("readings")]
    public async Task<IActionResult> AddReading([FromBody] EnergyReading reading)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        reading.Id = 0;
        reading.Timestamp = reading.Timestamp == default ? DateTime.UtcNow : reading.Timestamp.ToUniversalTime();
        _db.EnergyReadings.Add(reading);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetLatest), new { id = reading.Id }, reading);
    }
}
