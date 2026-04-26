namespace PredictiveGuard.Data.Models;

public class SensorReading
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public DateTime Timestamp { get; set; }
    public double Temperature { get; set; } // °C
    public double Vibration { get; set; } // m/s²
    public double Load { get; set; } // %

    // Navigation
    public Asset Asset { get; set; }
}
