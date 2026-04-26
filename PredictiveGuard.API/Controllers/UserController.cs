using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PredictiveGuard.Data.Data;
using PredictiveGuard.Data.Models;

namespace PredictiveGuard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public UserController(ApplicationDbContext db) => _db = db;

    // GET /api/user
    [HttpGet]
    public async Task<ActionResult<List<User>>> GetUsers()
    {
        var users = await _db.Users.ToListAsync();
        return Ok(users);
    }

    // POST /api/user
    [HttpPost]
    public async Task<ActionResult<User>> CreateUser([FromBody] CreateUserDto dto)
    {
        // Check if user exists by GoogleId
        var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.GoogleId == dto.GoogleId);
        if (existingUser != null)
            return Ok(existingUser); // Return existing user

        var user = new User
        {
            GoogleId = dto.GoogleId,
            Email = dto.Email,
            FullName = dto.FullName,
            ProfilePictureUrl = dto.ProfilePictureUrl,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }

    // GET /api/user/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUserById(int id)
    {
        var user = await _db.Users
            .Include(u => u.AssetTeamMemberships)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound();

        return Ok(user);
    }
}

public class CreateUserDto
{
    public string GoogleId { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public string ProfilePictureUrl { get; set; }
}
