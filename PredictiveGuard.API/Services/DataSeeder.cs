using PredictiveGuard.Data.Data;
using PredictiveGuard.Data.Models;

namespace PredictiveGuard.API.Services;

public class DataSeeder
{
    private readonly ApplicationDbContext _db;

    public DataSeeder(ApplicationDbContext db) => _db = db;

    public async Task SeedAsync()
    {
        if (_db.Assets.Count() >= 30)
            return; // Already seeded

        var users = _db.Users.ToList();
        if (!users.Any())
        {
            users.Add(new User { GoogleId = "user1", Email = "engineer1@example.com", FullName = "Alice Engineer", CreatedAt = DateTime.UtcNow });
            users.Add(new User { GoogleId = "user2", Email = "engineer2@example.com", FullName = "Bob Technician", CreatedAt = DateTime.UtcNow });
            users.Add(new User { GoogleId = "user3", Email = "chief@example.com", FullName = "Charlie Chief", CreatedAt = DateTime.UtcNow });
            _db.Users.AddRange(users);
            await _db.SaveChangesAsync();
        }

        var types = new[] { "Wind Turbine", "CNC Machine", "HVAC Compressor", "Conveyor Belt", "Hydraulic Press", "Industrial Oven", "Pump System", "Generator" };
        var locations = new[] { "Site A - North", "Site B - South", "Site C - East", "Site D - West", "Offshore Platform 1", "Main Plant", "Assembly Line 1", "Packaging Facility" };

        var newAssets = new List<Asset>();
        int needed = 40 - _db.Assets.Count();
        
        for (int i = 0; i < needed; i++)
        {
            var type = types[Random.Shared.Next(types.Length)];
            var loc = locations[Random.Shared.Next(locations.Length)];
            
            var asset = new Asset
            {
                Name = $"{type} #{Random.Shared.Next(1000, 9999)}",
                Location = loc,
                Type = type,
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(10, 365)),
                IsActive = true
            };
            
            _db.Assets.Add(asset);
            newAssets.Add(asset);
        }
        
        await _db.SaveChangesAsync();

        var teamMembers = new List<AssetTeamMember>();
        var readings = new List<SensorReading>();
        var now = DateTime.UtcNow;

        foreach (var asset in newAssets)
        {
            // Assign 1-2 random users
            var assignedUser1 = users[Random.Shared.Next(users.Count)];
            teamMembers.Add(new AssetTeamMember { AssetId = asset.Id, UserId = assignedUser1.Id, Role = "Lead Engineer", JoinedAt = now.AddDays(-10) });
            
            if (Random.Shared.NextDouble() > 0.5)
            {
                var assignedUser2 = users[Random.Shared.Next(users.Count)];
                if (assignedUser1.Id != assignedUser2.Id)
                {
                    teamMembers.Add(new AssetTeamMember { AssetId = asset.Id, UserId = assignedUser2.Id, Role = "Technician", JoinedAt = now.AddDays(-5) });
                }
            }

            // Generate initial history (24 hours)
            double baseTemp = 40 + Random.Shared.NextDouble() * 30; // 40-70
            double baseVib = 1.0 + Random.Shared.NextDouble() * 2.0; // 1.0-3.0
            
            for (int i = 0; i < 24; i++)
            {
                readings.Add(new SensorReading
                {
                    AssetId = asset.Id,
                    Timestamp = now.AddHours(-i),
                    Temperature = baseTemp + (Random.Shared.NextDouble() * 10 - 5),
                    Vibration = baseVib + (Random.Shared.NextDouble() * 1.0 - 0.5),
                    Load = 40 + Random.Shared.Next(0, 50)
                });
            }
        }

        _db.AssetTeamMembers.AddRange(teamMembers);
        _db.SensorReadings.AddRange(readings);
        await _db.SaveChangesAsync();
    }
}