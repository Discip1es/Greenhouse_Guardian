using GreenhouseGuardian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace GreenhouseGuardian.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        if (await context.Sensors.AnyAsync())
            return; // Database already seeded

        // Create zones
        var zones = new List<Zone>
        {
            new Zone { Id = "zone-1", Name = "Tomato Section A", Description = "Main tomato growing area" },
            new Zone { Id = "zone-2", Name = "Cucumber Section B", Description = "Cucumber greenhouse" },
            new Zone { Id = "zone-3", Name = "Pepper Section C", Description = "Bell pepper area" }
        };
        await context.Zones.AddRangeAsync(zones);
        await context.SaveChangesAsync();

        // Create plant profiles
        var profiles = new List<PlantProfile>
        {
            new PlantProfile
            {
                Name = "Tomato Standard",
                CultureName = "Tomatoes",
                TempDayMin = 20, TempDayMax = 26,
                TempNightMin = 16, TempNightMax = 20,
                HumidityMin = 60, HumidityMax = 80,
                LightMin = 20000, LightMax = 40000,
                CO2Min = 400, CO2Max = 1000,
                PHMin = 6.0m, PHMax = 6.8m,
                ECMin = 2.0m, ECMax = 3.5m
            },
            new PlantProfile
            {
                Name = "Cucumber Standard",
                CultureName = "Cucumbers",
                TempDayMin = 22, TempDayMax = 28,
                TempNightMin = 18, TempNightMax = 22,
                HumidityMin = 70, HumidityMax = 90,
                LightMin = 15000, LightMax = 35000,
                CO2Min = 400, CO2Max = 1200,
                PHMin = 5.8m, PHMax = 6.5m,
                ECMin = 1.8m, ECMax = 3.0m
            }
        };
        await context.PlantProfiles.AddRangeAsync(profiles);
        await context.SaveChangesAsync();

        // Create sensors (100 sensors across zones)
        var sensorTypes = Enum.GetValues<SensorType>();
        var units = new Dictionary<SensorType, string>
        {
            { SensorType.TemperatureAir, "°C" },
            { SensorType.HumidityAir, "%" },
            { SensorType.SoilMoisture, "%" },
            { SensorType.Luminosity, "lux" },
            { SensorType.CO2, "ppm" },
            { SensorType.pH, "pH" },
            { SensorType.EC, "mS/cm" }
        };

        var sensors = new List<Sensor>();
        var random = new Random(42); // Fixed seed for reproducibility

        for (int i = 1; i <= 100; i++)
        {
            var zoneId = zones[i % zones.Count].Id;
            var type = sensorTypes.GetValue(i % sensorTypes.Length)!.As<SensorType>();
            
            sensors.Add(new Sensor
            {
                Id = i,
                Name = $"{type} Sensor {i}",
                Code = $"SENS-{i:D4}",
                Type = type,
                ZoneId = zoneId,
                Unit = units[type],
                WarningThresholdLow = GetWarningLow(type),
                WarningThresholdHigh = GetWarningHigh(type),
                CriticalThresholdLow = GetCriticalLow(type),
                CriticalThresholdHigh = GetCriticalHigh(type),
                IsActive = true
            });
        }
        await context.Sensors.AddRangeAsync(sensors);
        await context.SaveChangesAsync();

        // Create actuators
        var actuatorTypes = Enum.GetValues<ActuatorType>();
        var actuators = new List<Actuator>();

        for (int i = 1; i <= 20; i++)
        {
            var zoneId = zones[i % zones.Count].Id;
            var type = actuatorTypes.GetValue(i % actuatorTypes.Length)!.As<ActuatorType>();

            actuators.Add(new Actuator
            {
                Id = i,
                Name = $"{type} Actuator {i}",
                Code = $"ACT-{i:D4}",
                Type = type,
                ZoneId = zoneId,
                IsActive = true,
                State = ActuatorState.Off,
                IsAutoMode = true
            });
        }
        await context.Actuators.AddRangeAsync(actuators);
        await context.SaveChangesAsync();

        // Create admin user
        var passwordHash = HashPassword("admin123");
        var adminUser = new ApplicationUser
        {
            Id = "user-admin-001",
            Email = "admin@greenhouse.local",
            UserName = "Administrator",
            PasswordHash = passwordHash,
            Role = UserRole.Administrator,
            IsActive = true
        };
        await context.Users.AddAsync(adminUser);
        await context.SaveChangesAsync();

        // Create sample automation rules
        var rules = new List<AutomationRule>
        {
            new AutomationRule
            {
                Name = "High Temperature Ventilation",
                Description = "Turn on ventilation when temperature exceeds 28°C",
                SensorId = 1,
                ConditionOperator = "gt",
                ThresholdValue = 28,
                DurationMinutes = 5,
                ActuatorId = 1,
                Action = "turn_on",
                IsActive = true
            },
            new AutomationRule
            {
                Name = "Low Humidity Misting",
                Description = "Turn on misting when humidity drops below 50%",
                SensorId = 2,
                ConditionOperator = "lt",
                ThresholdValue = 50,
                DurationMinutes = 3,
                ActuatorId = 6,
                Action = "turn_on",
                IsActive = true
            }
        };
        await context.AutomationRules.AddRangeAsync(rules);
        await context.SaveChangesAsync();
    }

    private static decimal GetWarningLow(SensorType type) => type switch
    {
        SensorType.TemperatureAir => 15,
        SensorType.HumidityAir => 40,
        SensorType.SoilMoisture => 20,
        SensorType.Luminosity => 5000,
        SensorType.CO2 => 300,
        SensorType.pH => 5.5m,
        SensorType.EC => 1.0m,
        _ => 0
    };

    private static decimal GetWarningHigh(SensorType type) => type switch
    {
        SensorType.TemperatureAir => 32,
        SensorType.HumidityAir => 85,
        SensorType.SoilMoisture => 80,
        SensorType.Luminosity => 50000,
        SensorType.CO2 => 1500,
        SensorType.pH => 7.5m,
        SensorType.EC => 4.0m,
        _ => 100
    };

    private static decimal GetCriticalLow(SensorType type) => type switch
    {
        SensorType.TemperatureAir => 10,
        SensorType.HumidityAir => 30,
        SensorType.SoilMoisture => 10,
        SensorType.Luminosity => 1000,
        SensorType.CO2 => 200,
        SensorType.pH => 4.5m,
        SensorType.EC => 0.5m,
        _ => 0
    };

    private static decimal GetCriticalHigh(SensorType type) => type switch
    {
        SensorType.TemperatureAir => 40,
        SensorType.HumidityAir => 95,
        SensorType.SoilMoisture => 95,
        SensorType.Luminosity => 80000,
        SensorType.CO2 => 2000,
        SensorType.pH => 8.5m,
        SensorType.EC => 5.0m,
        _ => 100
    };

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
