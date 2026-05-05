using GreenhouseGuardian.Application.Interfaces;
using GreenhouseGuardian.Domain.Entities;
using GreenhouseGuardian.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenhouseGuardian.Infrastructure.Services;

public class PlantProfileService : IPlantProfileService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PlantProfileService> _logger;

    public PlantProfileService(ApplicationDbContext context, ILogger<PlantProfileService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<PlantProfile>> GetAllProfilesAsync()
    {
        return await _context.PlantProfiles
            .Include(p => p.Zones)
            .OrderBy(p => p.CultureName)
            .ToListAsync();
    }

    public async Task<PlantProfile?> GetProfileByIdAsync(int id)
    {
        return await _context.PlantProfiles
            .Include(p => p.Zones)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PlantProfile> CreateProfileAsync(PlantProfile profile)
    {
        _context.PlantProfiles.Add(profile);
        await _context.SaveChangesAsync();
        _logger.LogInformation("PlantProfile {CultureName} created", profile.CultureName);
        return profile;
    }

    public async Task<PlantProfile> UpdateProfileAsync(PlantProfile profile)
    {
        var existing = await _context.PlantProfiles.FindAsync(profile.Id);
        if (existing == null) throw new InvalidOperationException($"PlantProfile {profile.Id} not found");

        existing.Name = profile.Name;
        existing.Description = profile.Description;
        existing.CultureName = profile.CultureName;
        existing.TempDayMin = profile.TempDayMin;
        existing.TempDayMax = profile.TempDayMax;
        existing.TempNightMin = profile.TempNightMin;
        existing.TempNightMax = profile.TempNightMax;
        existing.HumidityMin = profile.HumidityMin;
        existing.HumidityMax = profile.HumidityMax;
        existing.LightMin = profile.LightMin;
        existing.LightMax = profile.LightMax;
        existing.CO2Min = profile.CO2Min;
        existing.CO2Max = profile.CO2Max;
        existing.PHMin = profile.PHMin;
        existing.PHMax = profile.PHMax;
        existing.ECMin = profile.ECMin;
        existing.ECMax = profile.ECMax;

        await _context.SaveChangesAsync();
        _logger.LogInformation("PlantProfile {CultureName} updated", existing.CultureName);
        return existing;
    }

    public async Task DeleteProfileAsync(int id)
    {
        var profile = await _context.PlantProfiles.FindAsync(id);
        if (profile != null)
        {
            _context.PlantProfiles.Remove(profile);
            await _context.SaveChangesAsync();
            _logger.LogInformation("PlantProfile {CultureName} deleted", profile.CultureName);
        }
    }

    public async Task<IEnumerable<string>> GetRecommendationsAsync(string zoneId)
    {
        var zone = await _context.Zones
            .Include(z => z.PlantProfile)
            .Include(z => z.Sensors)
            .FirstOrDefaultAsync(z => z.Id == zoneId);

        if (zone?.PlantProfile == null)
            return new List<string> { "Zone or plant profile not found" };

        var recommendations = new List<string>();
        var profile = zone.PlantProfile;

        // Get current sensor readings for this zone
        var zoneSensors = zone.Sensors.ToList();
        
        // Check temperature recommendations
        var tempSensor = zoneSensors.FirstOrDefault(s => s.Type == SensorType.TemperatureAir);
        if (tempSensor != null)
        {
            var hour = DateTime.UtcNow.Hour;
            var isDayTime = hour >= 6 && hour < 20;
            var targetMin = isDayTime ? profile.TempDayMin : profile.TempNightMin;
            var targetMax = isDayTime ? profile.TempDayMax : profile.TempNightMax;
            
            recommendations.Add($"Temperature: Target range for {(isDayTime ? "day" : "night")} is {targetMin}-{targetMax}°C");
        }

        // Humidity recommendations
        var humiditySensor = zoneSensors.FirstOrDefault(s => s.Type == SensorType.HumidityAir);
        if (humiditySensor != null)
        {
            recommendations.Add($"Humidity: Target range is {profile.HumidityMin}-{profile.HumidityMax}%");
        }

        // Light recommendations
        var lightSensor = zoneSensors.FirstOrDefault(s => s.Type == SensorType.Luminosity);
        if (lightSensor != null)
        {
            recommendations.Add($"Light intensity: Target range is {profile.LightMin}-{profile.LightMax} lux");
        }

        // CO2 recommendations
        var co2Sensor = zoneSensors.FirstOrDefault(s => s.Type == SensorType.CO2);
        if (co2Sensor != null)
        {
            recommendations.Add($"CO2 concentration: Target range is {profile.CO2Min}-{profile.CO2Max} ppm");
        }

        // pH recommendations
        var phSensor = zoneSensors.FirstOrDefault(s => s.Type == SensorType.pH);
        if (phSensor != null)
        {
            recommendations.Add($"pH level: Target range is {profile.PHMin}-{profile.PHMax}");
        }

        // EC recommendations
        var ecSensor = zoneSensors.FirstOrDefault(s => s.Type == SensorType.EC);
        if (ecSensor != null)
        {
            recommendations.Add($"EC level: Target range is {profile.ECMin}-{profile.ECMax} mS/cm");
        }

        return recommendations;
    }
}
