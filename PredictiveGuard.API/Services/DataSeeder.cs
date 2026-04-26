using PredictiveGuard.Data.Data;
using PredictiveGuard.Data.Models;

namespace PredictiveGuard.API.Services;

public class DataSeeder
{
    private readonly ApplicationDbContext _db;

    public DataSeeder(ApplicationDbContext db) => _db = db;

    public async Task SeedAsync()
    {
        if (_db.Assets.Any())
            return; // Already seeded

        // Create users
        var user1 = new User
        {
            GoogleId = "user1",
            Email = "engineer1@example.com",
            FullName = "Alice Engineer",
            CreatedAt = DateTime.UtcNow
        };

        var user2 = new User
        {
            GoogleId = "user2",
            Email = "engineer2@example.com",
            FullName = "Bob Technician",
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.AddRange(user1, user2);
        await _db.SaveChangesAsync();

        // Create asset
        var asset = new Asset
        {
            Name = "Wind Turbine #1",
            Location = "Site A",
            Type = "Wind Turbine",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _db.Assets.Add(asset);
        await _db.SaveChangesAsync();

        // Add team members
        _db.AssetTeamMembers.AddRange(
            new AssetTeamMember { AssetId = asset.Id, UserId = user1.Id, Role = "Engineer", JoinedAt = DateTime.UtcNow },
            new AssetTeamMember { AssetId = asset.Id, UserId = user2.Id, Role = "Technician", JoinedAt = DateTime.UtcNow }
        );

        // Add sample sensor readings
        var readings = new List<SensorReading>();
        var now = DateTime.UtcNow;
        for (int i = 0; i < 24; i++)
        {
            readings.Add(new SensorReading
            {
                AssetId = asset.Id,
                Timestamp = now.AddHours(-i),
                Temperature = 65 + Random.Shared.Next(0, 15),
                Vibration = 2.0 + Random.Shared.NextDouble() * 2.0,
                Load = 50 + Random.Shared.Next(0, 30)
            });
        }

        _db.SensorReadings.AddRange(readings);
        await _db.SaveChangesAsync();
    }
}