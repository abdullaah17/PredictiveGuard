using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PredictiveGuard.Data.Data;
using PredictiveGuard.Data.Models;

namespace PredictiveGuard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssetController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AssetController(ApplicationDbContext db) => _db = db;

    // GET /api/asset
    [HttpGet]
    public async Task<ActionResult<List<Asset>>> GetAssets()
    {
        var assets = await _db.Assets
            .Include(a => a.TeamMembers)
            .ToListAsync();
        return Ok(assets);
    }

    // GET /api/asset/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Asset>> GetAssetById(int id)
    {
        var asset = await _db.Assets
            .Include(a => a.TeamMembers)
            .Include(a => a.SensorReadings.OrderByDescending(sr => sr.Timestamp).Take(100))
            .FirstOrDefaultAsync(a => a.Id == id);

        if (asset == null)
            return NotFound();

        return Ok(asset);
    }

    // POST /api/asset
    [HttpPost]
    public async Task<ActionResult<Asset>> CreateAsset([FromBody] CreateAssetDto dto)
    {
        var asset = new Asset
        {
            Name = dto.Name,
            Location = dto.Location,
            Type = dto.Type,
            CreatedAt = DateTime.UtcNow
        };

        _db.Assets.Add(asset);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAssetById), new { id = asset.Id }, asset);
    }
}

public class CreateAssetDto
{
    public string Name { get; set; }
    public string Location { get; set; }
    public string Type { get; set; }
}
