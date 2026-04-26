namespace PredictiveGuard.Data.Models;

public class AssetTeamMember
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public int UserId { get; set; }
    public string Role { get; set; } // "Engineer", "Technician", "Lead"
    public DateTime JoinedAt { get; set; }

    // Navigation
    public Asset Asset { get; set; }
    public User User { get; set; }
}
