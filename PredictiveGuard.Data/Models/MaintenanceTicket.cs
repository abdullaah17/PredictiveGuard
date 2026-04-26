namespace PredictiveGuard.Data.Models;

public class MaintenanceTicket
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public int? AssignedToUserId { get; set; }
    public string Status { get; set; } // "Reported", "Assigned", "In Progress", "Completed"
    public string AlertType { get; set; } // "Temperature", "Vibration", "Trend"
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? Version { get; set; } // For Optimistic Concurrency

    // Navigation
    public Asset Asset { get; set; }
    public User AssignedToUser { get; set; }
}
