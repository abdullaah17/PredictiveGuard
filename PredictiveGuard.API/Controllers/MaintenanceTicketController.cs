using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PredictiveGuard.Data.Data;
using PredictiveGuard.Data.Models;

namespace PredictiveGuard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaintenanceTicketController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public MaintenanceTicketController(ApplicationDbContext db) => _db = db;

    // GET /api/maintenanceticket
    [HttpGet]
    public async Task<ActionResult<List<MaintenanceTicket>>> GetTickets([FromQuery] string? status = null)
    {
        var query = _db.MaintenanceTickets
            .Include(mt => mt.Asset)
            .Include(mt => mt.AssignedToUser)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(mt => mt.Status == status);

        var tickets = await query.OrderByDescending(mt => mt.CreatedAt).ToListAsync();
        return Ok(tickets);
    }

    // GET /api/maintenanceticket/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<MaintenanceTicket>> GetTicketById(int id)
    {
        var ticket = await _db.MaintenanceTickets
            .Include(mt => mt.Asset)
            .Include(mt => mt.AssignedToUser)
            .FirstOrDefaultAsync(mt => mt.Id == id);

        if (ticket == null)
            return NotFound();

        return Ok(ticket);
    }

    // PATCH /api/maintenanceticket/{id}/assign
    [HttpPatch("{id}/assign")]
    public async Task<ActionResult<MaintenanceTicket>> AssignTicket(int id, [FromBody] AssignTicketDto dto)
    {
        var ticket = await _db.MaintenanceTickets.FindAsync(id);
        if (ticket == null)
            return NotFound();

        // Check user exists
        var user = await _db.Users.FindAsync(dto.UserId);
        if (user == null)
            return BadRequest("User not found");

        ticket.AssignedToUserId = dto.UserId;
        ticket.Status = "Assigned";
        ticket.Version++;

        _db.MaintenanceTickets.Update(ticket);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("Ticket was modified by another user");
        }

        return Ok(ticket);
    }

    // PATCH /api/maintenanceticket/{id}/status
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<MaintenanceTicket>> UpdateTicketStatus(int id, [FromBody] UpdateStatusDto dto)
    {
        var ticket = await _db.MaintenanceTickets.FindAsync(id);
        if (ticket == null)
            return NotFound();

        ticket.Status = dto.Status;
        if (dto.Status == "Completed")
            ticket.CompletedAt = DateTime.UtcNow;
        ticket.Version++;

        _db.MaintenanceTickets.Update(ticket);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("Ticket was modified by another user");
        }

        return Ok(ticket);
    }
}

public class AssignTicketDto
{
    public int UserId { get; set; }
}

public class UpdateStatusDto
{
    public string Status { get; set; }
}
