using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PredictiveGuard.Data.Data;
using PredictiveGuard.Data.Models;

namespace PredictiveGuard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SensorReadingController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public SensorReadingController(ApplicationDbContext db) => _db = db;

    // GET /api/sensorreading/{assetId}?hours=24
    [HttpGet("{assetId}")]
    public async Task<ActionResult<List<SensorReading>>> GetReadings(int assetId, [FromQuery] int hours = 24)
    {
        var since = DateTime.UtcNow.AddHours(-hours);
        var readings = await _db.SensorReadings
            .Where(sr => sr.AssetId == assetId && sr.Timestamp >= since)
            .OrderByDescending(sr => sr.Timestamp)
            .ToListAsync();

        return Ok(readings);
    }

    // POST /api/sensorreading
    [HttpPost]
    public async Task<ActionResult<SensorReading>> CreateReading([FromBody] CreateSensorReadingDto dto)
    {
        var asset = await _db.Assets.FindAsync(dto.AssetId);
        if (asset == null)
            return NotFound("Asset not found");

        var reading = new SensorReading
        {
            AssetId = dto.AssetId,
            Timestamp = DateTime.UtcNow,
            Temperature = dto.Temperature,
            Vibration = dto.Vibration,
            Load = dto.Load
        };

        _db.SensorReadings.Add(reading);
        await _db.SaveChangesAsync();

        // Check for alerts
        await CheckAlertsAsync(dto.AssetId);

        return CreatedAtAction(nameof(GetReadings), new { assetId = reading.AssetId }, reading);
    }

    // POST /api/sensorreading/batch (for bulk ingestion)
    [HttpPost("batch")]
    public async Task<ActionResult> CreateBatchReadings([FromBody] List<CreateSensorReadingDto> dtos)
    {
        foreach (var dto in dtos)
        {
            var reading = new SensorReading
            {
                AssetId = dto.AssetId,
                Timestamp = DateTime.UtcNow,
                Temperature = dto.Temperature,
                Vibration = dto.Vibration,
                Load = dto.Load
            };
            _db.SensorReadings.Add(reading);
        }

        await _db.SaveChangesAsync();

        // Check alerts for all assets
        var assetIds = dtos.Select(d => d.AssetId).Distinct();
        foreach (var assetId in assetIds)
        {
            await CheckAlertsAsync(assetId);
        }

        return Ok("Batch inserted");
    }

    // Private method: Check for threshold breaches
    private async Task CheckAlertsAsync(int assetId)
    {
        var latestReading = await _db.SensorReadings
            .Where(sr => sr.AssetId == assetId)
            .OrderByDescending(sr => sr.Timestamp)
            .FirstOrDefaultAsync();

        if (latestReading == null)
            return;

        // Threshold logic
        if (latestReading.Temperature > 80)
        {
            await CreateTicketIfNotExists(assetId, "Temperature", $"Temperature alert: {latestReading.Temperature}°C");
        }

        if (latestReading.Vibration > 5.0)
        {
            await CreateTicketIfNotExists(assetId, "Vibration", $"Vibration alert: {latestReading.Vibration} m/s²");
        }

        // Trend analysis: last 10 readings, temp rising 2°C/hour?
        var lastTenReadings = await _db.SensorReadings
            .Where(sr => sr.AssetId == assetId)
            .OrderByDescending(sr => sr.Timestamp)
            .Take(10)
            .ToListAsync();

        if (lastTenReadings.Count >= 2)
        {
            var oldest = lastTenReadings.Last();
            var newest = lastTenReadings.First();
            var timeDiffHours = (newest.Timestamp - oldest.Timestamp).TotalHours;
            var tempDiff = newest.Temperature - oldest.Temperature;

            if (timeDiffHours > 0 && (tempDiff / timeDiffHours) > 2.0)
            {
                await CreateTicketIfNotExists(assetId, "Trend", $"Temperature rising at {tempDiff / timeDiffHours:F2}°C/hour");
            }
        }
    }

    // Private method: Create ticket only if one doesn't exist
    private async Task CreateTicketIfNotExists(int assetId, string alertType, string description)
    {
        var existingTicket = await _db.MaintenanceTickets
            .Where(mt => mt.AssetId == assetId && mt.AlertType == alertType && mt.Status != "Completed")
            .FirstOrDefaultAsync();

        if (existingTicket == null)
        {
            var ticket = new MaintenanceTicket
            {
                AssetId = assetId,
                AlertType = alertType,
                Status = "Reported",
                Description = description,
                CreatedAt = DateTime.UtcNow
            };
            _db.MaintenanceTickets.Add(ticket);
            await _db.SaveChangesAsync();
        }
    }
}

public class CreateSensorReadingDto
{
    public int AssetId { get; set; }
    public double Temperature { get; set; }
    public double Vibration { get; set; }
    public double Load { get; set; }
}
