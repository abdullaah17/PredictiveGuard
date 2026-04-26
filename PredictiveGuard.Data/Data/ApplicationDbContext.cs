using Microsoft.EntityFrameworkCore;
using PredictiveGuard.Data.Models;

namespace PredictiveGuard.Data.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<AssetTeamMember> AssetTeamMembers { get; set; }
    public DbSet<SensorReading> SensorReadings { get; set; }
    public DbSet<MaintenanceTicket> MaintenanceTickets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Asset
        modelBuilder.Entity<Asset>()
            .HasKey(a => a.Id);
        modelBuilder.Entity<Asset>()
            .HasMany(a => a.SensorReadings)
            .WithOne(s => s.Asset)
            .HasForeignKey(s => s.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        // User
        modelBuilder.Entity<User>()
            .HasIndex(u => u.GoogleId)
            .IsUnique();
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // AssetTeamMember
        modelBuilder.Entity<AssetTeamMember>()
            .HasOne(atm => atm.Asset)
            .WithMany(a => a.TeamMembers)
            .HasForeignKey(atm => atm.AssetId);
        modelBuilder.Entity<AssetTeamMember>()
            .HasOne(atm => atm.User)
            .WithMany(u => u.AssetTeamMemberships)
            .HasForeignKey(atm => atm.UserId);

        // MaintenanceTicket
        modelBuilder.Entity<MaintenanceTicket>()
            .HasOne(mt => mt.Asset)
            .WithMany(a => a.MaintenanceTickets)
            .HasForeignKey(mt => mt.AssetId);
        modelBuilder.Entity<MaintenanceTicket>()
            .HasOne(mt => mt.AssignedToUser)
            .WithMany(u => u.AssignedTickets)
            .HasForeignKey(mt => mt.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Concurrency Token
        modelBuilder.Entity<MaintenanceTicket>()
            .Property(mt => mt.Version)
            .IsConcurrencyToken();

        // SensorReading indexes (for time-series queries)
        modelBuilder.Entity<SensorReading>()
            .HasIndex(sr => new { sr.AssetId, sr.Timestamp })
            .IsUnique();
    }
}
