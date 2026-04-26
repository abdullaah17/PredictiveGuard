namespace PredictiveGuard.Data.Models;

public class User
{
    public int Id { get; set; }
    public string GoogleId { get; set; } // From Google Auth
    public string Email { get; set; }
    public string FullName { get; set; }
    public string ProfilePictureUrl { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public ICollection<AssetTeamMember> AssetTeamMemberships { get; set; } = new List<AssetTeamMember>();
    public ICollection<MaintenanceTicket> AssignedTickets { get; set; } = new List<MaintenanceTicket>();
}
