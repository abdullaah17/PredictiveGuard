using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PredictiveGuard.Data.Data;
using PredictiveGuard.Data.Models;

namespace PredictiveGuard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssetTeamMemberController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AssetTeamMemberController(ApplicationDbContext db) => _db = db;

    // POST /api/assetteammember
    [HttpPost]
    public async Task<ActionResult<AssetTeamMember>> AddTeamMember([FromBody] AddTeamMemberDto dto)
    {
        var asset = await _db.Assets.FindAsync(dto.AssetId);
        if (asset == null)
            return NotFound("Asset not found");

        var user = await _db.Users.FindAsync(dto.UserId);
        if (user == null)
            return NotFound("User not found");

        // Check if already a member
        var existing = await _db.AssetTeamMembers
            .FirstOrDefaultAsync(atm => atm.AssetId == dto.AssetId && atm.UserId == dto.UserId);
        if (existing != null)
            return BadRequest("User already a team member");

        var teamMember = new AssetTeamMember
        {
            AssetId = dto.AssetId,
            UserId = dto.UserId,
            Role = dto.Role ?? "Engineer",
            JoinedAt = DateTime.UtcNow
        };

        _db.AssetTeamMembers.Add(teamMember);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTeamMembers), new { assetId = dto.AssetId }, teamMember);
    }

    // GET /api/assetteammember/{assetId}
    [HttpGet("{assetId}")]
    public async Task<ActionResult<List<AssetTeamMember>>> GetTeamMembers(int assetId)
    {
        var members = await _db.AssetTeamMembers
            .Where(atm => atm.AssetId == assetId)
            .Include(atm => atm.User)
            .ToListAsync();

        return Ok(members);
    }
}

public class AddTeamMemberDto
{
    public int AssetId { get; set; }
    public int UserId { get; set; }
    public string Role { get; set; } = "Engineer";
}
