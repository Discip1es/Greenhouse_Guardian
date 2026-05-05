using GreenhouseGuardian.Application.Interfaces;
using GreenhouseGuardian.Domain.Entities;
using GreenhouseGuardian.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenhouseGuardian.Infrastructure.Services;

public class SensorService : ISensorService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentStateCache _cache;
    private readonly ILogger<SensorService> _logger;

    public SensorService(ApplicationDbContext context, ICurrentStateCache cache, ILogger<SensorService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<Sensor>> GetAllSensorsAsync()
    {
        return await _context.Sensors
            .Include(s => s.Zone)
            .OrderBy(s => s.Code)
            .ToListAsync();
    }

    public async Task<Sensor?> GetSensorByIdAsync(int id)
    {
        return await _context.Sensors
            .Include(s => s.Zone)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Sensor> CreateSensorAsync(Sensor sensor)
    {
        sensor.Code = sensor.Code.Trim().ToUpper();
        _context.Sensors.Add(sensor);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Sensor {SensorCode} created", sensor.Code);
        return sensor;
    }

    public async Task<Sensor> UpdateSensorAsync(Sensor sensor)
    {
        var existing = await _context.Sensors.FindAsync(sensor.Id);
        if (existing == null) throw new InvalidOperationException($"Sensor {sensor.Id} not found");

        existing.Name = sensor.Name;
        existing.Type = sensor.Type;
        existing.ZoneId = sensor.ZoneId;
        existing.WarningThresholdLow = sensor.WarningThresholdLow;
        existing.WarningThresholdHigh = sensor.WarningThresholdHigh;
        existing.CriticalThresholdLow = sensor.CriticalThresholdLow;
        existing.CriticalThresholdHigh = sensor.CriticalThresholdHigh;
        existing.Unit = sensor.Unit;
        existing.IsActive = sensor.IsActive;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Sensor {SensorCode} updated", existing.Code);
        return existing;
    }

    public async Task DeleteSensorAsync(int id)
    {
        var sensor = await _context.Sensors.FindAsync(id);
        if (sensor != null)
        {
            _context.Sensors.Remove(sensor);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Sensor {SensorCode} deleted", sensor.Code);
        }
    }

    public async Task<IEnumerable<SensorReading>> GetSensorHistoryAsync(int sensorId, DateTime from, DateTime to, string? interval = null)
    {
        var query = _context.SensorReadings
            .Where(r => r.SensorId == sensorId && r.Timestamp >= from && r.Timestamp <= to)
            .OrderBy(r => r.Timestamp);

        // Apply aggregation if interval specified
        if (!string.IsNullOrEmpty(interval))
        {
            query = interval.ToLower() switch
            {
                "minute" => query.GroupBy(r => new { r.SensorId, Minute = r.Timestamp.Date.AddHours(r.Timestamp.Hour).AddMinutes(r.Timestamp.Minute) })
                    .Select(g => new SensorReading 
                    { 
                        SensorId = g.Key.SensorId, 
                        Timestamp = g.Key.Minute, 
                        Value = g.Average(r => r.Value),
                        Quality = "Aggregated"
                    }),
                "hour" => query.GroupBy(r => new { r.SensorId, Hour = r.Timestamp.Date.AddHours(r.Timestamp.Hour) })
                    .Select(g => new SensorReading 
                    { 
                        SensorId = g.Key.SensorId, 
                        Timestamp = g.Key.Hour, 
                        Value = g.Average(r => r.Value),
                        Quality = "Aggregated"
                    }),
                "day" => query.GroupBy(r => new { r.SensorId, Day = r.Timestamp.Date })
                    .Select(g => new SensorReading 
                    { 
                        SensorId = g.Key.SensorId, 
                        Timestamp = g.Key.Day, 
                        Value = g.Average(r => r.Value),
                        Quality = "Aggregated"
                    }),
                _ => query
            };
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<SensorReading>> GetCurrentReadingsAsync()
    {
        var sensorValues = await _cache.GetAllSensorCurrentValuesAsync();
        var now = DateTime.UtcNow;
        
        return sensorValues.Select(kv => new SensorReading
        {
            SensorId = kv.Key,
            Value = kv.Value,
            Timestamp = now,
            Quality = "Good"
        });
    }
}
