using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PredictiveGuard.Data.Data;
using PredictiveGuard.Data.Models;

namespace PredictiveGuard.API.Services;

public class SensorDataSimulatorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SensorDataSimulatorService> _logger;

    public SensorDataSimulatorService(IServiceProvider serviceProvider, ILogger<SensorDataSimulatorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Sensor Data Simulator Background Service is starting.");

        var random = new Random();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                var assets = dbContext.Assets.ToList();
                if (assets.Any())
                {
                    foreach (var asset in assets)
                    {
                        // Get latest reading to base new reading on
                        var lastReading = dbContext.SensorReadings
                            .Where(sr => sr.AssetId == asset.Id)
                            .OrderByDescending(sr => sr.Timestamp)
                            .FirstOrDefault();

                        double newTemp, newVib, newLoad;

                        if (lastReading != null)
                        {
                            // Random walk to simulate real continuous data
                            newTemp = Math.Max(20, Math.Min(120, lastReading.Temperature + (random.NextDouble() * 4 - 2)));
                            newVib = Math.Max(0, Math.Min(10, lastReading.Vibration + (random.NextDouble() * 1 - 0.5)));
                            newLoad = Math.Max(0, Math.Min(100, lastReading.Load + (random.NextDouble() * 10 - 5)));
                        }
                        else
                        {
                            // Baseline start
                            newTemp = 45.0 + random.NextDouble() * 10;
                            newVib = 1.5 + random.NextDouble() * 2;
                            newLoad = 50.0 + random.NextDouble() * 20;
                        }

                        // Introduce occasional spikes (5% chance)
                        if (random.NextDouble() > 0.95)
                        {
                            newTemp += random.NextDouble() * 15; // Sudden temp spike
                            newVib += random.NextDouble() * 3;   // Sudden vibration spike
                        }

                        var newReading = new SensorReading
                        {
                            AssetId = asset.Id,
                            Timestamp = DateTime.UtcNow,
                            Temperature = Math.Round(newTemp, 2),
                            Vibration = Math.Round(newVib, 2),
                            Load = Math.Round(newLoad, 2)
                        };

                        dbContext.SensorReadings.Add(newReading);

                        // Threshold logic
                        if (newTemp > 80)
                        {
                            CreateTicketIfNotExists(dbContext, asset.Id, "Temperature", $"Temperature alert: {newReading.Temperature}°C");
                        }
                        if (newVib > 5.0)
                        {
                            CreateTicketIfNotExists(dbContext, asset.Id, "Vibration", $"Vibration alert: {newReading.Vibration} m/s²");
                        }
                    }

                    await dbContext.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing Sensor Data Simulator.");
            }

            // Wait 3 seconds before generating the next batch of data points
            await Task.Delay(3000, stoppingToken);
        }
        
        _logger.LogInformation("Sensor Data Simulator Background Service is stopping.");
    }

    private void CreateTicketIfNotExists(ApplicationDbContext dbContext, int assetId, string alertType, string description)
    {
        var existingTicket = dbContext.MaintenanceTickets
            .FirstOrDefault(mt => mt.AssetId == assetId && mt.AlertType == alertType && mt.Status != "Completed");

        if (existingTicket == null)
        {
            var ticket = new MaintenanceTicket
            {
                AssetId = assetId,
                AlertType = alertType,
                Status = "Reported",
                Description = description,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.MaintenanceTickets.Add(ticket);
        }
    }
}
