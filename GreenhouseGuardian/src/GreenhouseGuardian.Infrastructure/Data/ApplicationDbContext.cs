using GreenhouseGuardian.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenhouseGuardian.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Domain entities
    public DbSet<Sensor> Sensors { get; set; }
    public DbSet<SensorReading> SensorReadings { get; set; }
    public DbSet<Actuator> Actuators { get; set; }
    public DbSet<ActuatorLog> ActuatorLogs { get; set; }
    public DbSet<Zone> Zones { get; set; }
    public DbSet<PlantProfile> PlantProfiles { get; set; }
    public DbSet<AutomationRule> AutomationRules { get; set; }
    public DbSet<RuleExecutionLog> RuleExecutionLogs { get; set; }
    public DbSet<Alert> Alerts { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<NotificationChannel> NotificationChannels { get; set; }
    public DbSet<NotificationLog> NotificationLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Sensor configuration
        modelBuilder.Entity<Sensor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Unit).HasMaxLength(20);
            entity.HasOne(e => e.Zone).WithMany(z => z.Sensors).HasForeignKey(e => e.ZoneId).OnDelete(DeleteBehavior.SetNull);
        });

        // SensorReading configuration - TimescaleDB hypertable
        modelBuilder.Entity<SensorReading>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Value).HasPrecision(18, 4);
            entity.HasIndex(e => new { e.SensorId, e.Timestamp });
            entity.HasOne(e => e.Sensor).WithMany(s => s.Readings).HasForeignKey(e => e.SensorId).OnDelete(DeleteBehavior.Cascade);
        });

        // Actuator configuration
        modelBuilder.Entity<Actuator>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasOne(e => e.Zone).WithMany(z => z.Actuators).HasForeignKey(e => e.ZoneId).OnDelete(DeleteBehavior.SetNull);
        });

        // ActuatorLog configuration
        modelBuilder.Entity<ActuatorLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ActuatorId);
            entity.HasOne(e => e.Actuator).WithMany(a => a.CommandLogs).HasForeignKey(e => e.ActuatorId).OnDelete(DeleteBehavior.Cascade);
        });

        // Zone configuration
        modelBuilder.Entity<Zone>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.HasOne(e => e.PlantProfile).WithMany(p => p.Zones).HasForeignKey(e => e.PlantProfileId).OnDelete(DeleteBehavior.SetNull);
        });

        // PlantProfile configuration
        modelBuilder.Entity<PlantProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CultureName).HasMaxLength(100).IsRequired();
        });

        // AutomationRule configuration
        modelBuilder.Entity<AutomationRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Sensor).WithMany().HasForeignKey(e => e.SensorId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Actuator).WithMany().HasForeignKey(e => e.ActuatorId).OnDelete(DeleteBehavior.SetNull);
        });

        // RuleExecutionLog configuration
        modelBuilder.Entity<RuleExecutionLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RuleId);
            entity.HasOne(e => e.Rule).WithMany(r => r.ExecutionLogs).HasForeignKey(e => e.RuleId).OnDelete(DeleteBehavior.Cascade);
        });

        // Alert configuration
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.IsAcknowledged);
            entity.HasOne(e => e.Sensor).WithMany(s => s.Alerts).HasForeignKey(e => e.SensorId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Actuator).WithMany().HasForeignKey(e => e.ActuatorId).OnDelete(DeleteBehavior.SetNull);
        });

        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
        });

        // ApplicationUser configuration
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.UserName).HasMaxLength(256).IsRequired();
        });

        // RefreshToken configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasOne(e => e.User).WithMany(u => u.RefreshTokens).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // NotificationChannel configuration
        modelBuilder.Entity<NotificationChannel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // NotificationLog configuration
        modelBuilder.Entity<NotificationLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Alert).WithMany().HasForeignKey(e => e.AlertId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Channel).WithMany().HasForeignKey(e => e.ChannelId).OnDelete(DeleteBehavior.Cascade);
        });

        // Seed default admin user (password: Admin123!)
        var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");
        modelBuilder.Entity<ApplicationUser>().HasData(
            new ApplicationUser
            {
                Id = "admin-0000-0000-0000-000000000001",
                Email = "admin@greenhouse.local",
                UserName = "admin",
                PasswordHash = adminPasswordHash,
                Role = UserRole.Administrator,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );

        // Seed default plant profiles
        modelBuilder.Entity<PlantProfile>().HasData(
            new PlantProfile
            {
                Id = 1,
                Name = "Томаты стандарт",
                CultureName = "Томаты",
                Description = "Стандартные параметры для выращивания томатов",
                TempDayMin = 22, TempDayMax = 26,
                TempNightMin = 18, TempNightMax = 20,
                HumidityMin = 60, HumidityMax = 80,
                LightMin = 20000, LightMax = 60000,
                CO2Min = 400, CO2Max = 1000,
                PHMin = 6.0m, PHMax = 6.8m,
                ECMin = 2.0m, ECMax = 3.5m,
                CreatedAt = DateTime.UtcNow
            },
            new PlantProfile
            {
                Id = 2,
                Name = "Огурцы стандарт",
                CultureName = "Огурцы",
                Description = "Стандартные параметры для выращивания огурцов",
                TempDayMin = 24, TempDayMax = 28,
                TempNightMin = 20, TempNightMax = 22,
                HumidityMin = 70, HumidityMax = 90,
                LightMin = 15000, LightMax = 50000,
                CO2Min = 400, CO2Max = 1200,
                PHMin = 5.8m, PHMax = 6.5m,
                ECMin = 1.8m, ECMax = 3.0m,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
