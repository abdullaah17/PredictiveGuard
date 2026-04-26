namespace PredictiveGuard.Data.Models;

public class Asset
{
    public int Id { get; set; }
    public string Name { get; set; } // e.g., "Wind Turbine #1"
    public string Location { get; set; } // e.g., "Site A"
    public string Type { get; set; } // e.g., "Wind Turbine", "Transformer"
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<SensorReading> SensorReadings { get; set; } = new List<SensorReading>();
    public ICollection<MaintenanceTicket> MaintenanceTickets { get; set; } = new List<MaintenanceTicket>();
    public ICollection<AssetTeamMember> TeamMembers { get; set; } = new List<AssetTeamMember>();
}
