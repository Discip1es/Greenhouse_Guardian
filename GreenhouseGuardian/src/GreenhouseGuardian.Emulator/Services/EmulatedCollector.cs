using GreenhouseGuardian.Application.Interfaces;
using GreenhouseGuardian.Domain.Entities;
using GreenhouseGuardian.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenhouseGuardian.Emulator.Services;

public class EmulatedCollector : ITelemetryCollector, IDisposable
{
    private readonly ILogger<EmulatedCollector> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ICurrentStateCache _cache;
    private Timer? _collectionTimer;
    private bool _disposed;
    private readonly Random _random = new();
    
    // Sensor emulation profiles
    private readonly Dictionary<SensorType, SensorProfile> _sensorProfiles = new()
    {
        [SensorType.TemperatureAir] = new SensorProfile 
        { 
            BaseValue = 24, Amplitude = 5, PeriodHours = 24, PhaseOffset = -6, NoiseStdDev = 0.5, Unit = "°C",
            MinValue = 15, MaxValue = 35
        },
        [SensorType.HumidityAir] = new SensorProfile 
        { 
            BaseValue = 70, Amplitude = 15, PeriodHours = 24, PhaseOffset = 6, NoiseStdDev = 2, Unit = "%",
            MinValue = 40, MaxValue = 95
        },
        [SensorType.SoilMoisture] = new SensorProfile 
        { 
            BaseValue = 45, Amplitude = 20, PeriodHours = 48, PhaseOffset = 0, NoiseStdDev = 1, Unit = "%",
            MinValue = 20, MaxValue = 80
        },
        [SensorType.Luminosity] = new SensorProfile 
        { 
            BaseValue = 30000, Amplitude = 25000, PeriodHours = 24, PhaseOffset = -6, NoiseStdDev = 1000, Unit = "lux",
            MinValue = 0, MaxValue = 80000
        },
        [SensorType.CO2] = new SensorProfile 
        { 
            BaseValue = 600, Amplitude = 200, PeriodHours = 24, PhaseOffset = 0, NoiseStdDev = 30, Unit = "ppm",
            MinValue = 400, MaxValue = 1200
        },
        [SensorType.pH] = new SensorProfile 
        { 
            BaseValue = 6.5m, Amplitude = 0.3m, PeriodHours = 168, PhaseOffset = 0, NoiseStdDev = 0.05m, Unit = "pH",
            MinValue = 5.5m, MaxValue = 7.5m
        },
        [SensorType.EC] = new SensorProfile 
        { 
            BaseValue = 2.5m, Amplitude = 0.5m, PeriodHours = 168, PhaseOffset = 0, NoiseStdDev = 0.1m, Unit = "mS/cm",
            MinValue = 1.5m, MaxValue = 4.0m
        }
    };

    public EmulatedCollector(
        ILogger<EmulatedCollector> logger,
        IServiceProvider serviceProvider,
        ICurrentStateCache cache)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _cache = cache;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting emulated telemetry collector...");
        
        // Collect every 60 seconds as per spec
        _collectionTimer = new Timer(CollectAndStore, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping emulated telemetry collector...");
        _collectionTimer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private async void CollectAndStore(object? state)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var now = DateTime.UtcNow;
            var hourOfDay = now.Hour + now.Minute / 60.0;
            
            var readings = new List<SensorReading>();
            
            // Get all active sensors
            var sensors = await context.Sensors.Where(s => s.IsActive).ToListAsync();
            
            foreach (var sensor in sensors)
            {
                if (!_sensorProfiles.TryGetValue(sensor.Type, out var profile))
                    continue;
                
                // Calculate value based on sinusoidal model + noise
                var baseValue = (double)profile.BaseValue;
                var amplitude = (double)profile.Amplitude;
                var phaseOffset = profile.PhaseOffset;
                var period = profile.PeriodHours;
                
                // Sinusoidal component
                var sinusoidalValue = baseValue + amplitude * Math.Sin(2 * Math.PI * (hourOfDay + phaseOffset) / period);
                
                // Add Gaussian noise
                var noise = _random.NextGaussian(0, (double)profile.NoiseStdDev);
                var finalValue = sinusoidalValue + noise;
                
                // Clamp to min/max
                finalValue = Math.Max((double)profile.MinValue, Math.Min((double)profile.MaxValue, finalValue));
                
                var reading = new SensorReading
                {
                    SensorId = sensor.Id,
                    Value = (decimal)finalValue,
                    Timestamp = now,
                    Quality = "Good"
                };
                
                readings.Add(reading);
                
                // Update cache
                await _cache.SetSensorCurrentValueAsync(sensor.Id, (decimal)finalValue);
            }
            
            if (readings.Any())
            {
                await context.SensorReadings.AddRangeAsync(readings);
                await context.SaveChangesAsync();
                _logger.LogDebug($"Stored {readings.Count} sensor readings");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting emulated telemetry");
        }
    }

    public async Task<IEnumerable<SensorReading>> CollectLatestReadingsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var now = DateTime.UtcNow;
        var readings = new List<SensorReading>();
        
        var sensors = await context.Sensors.Where(s => s.IsActive).ToListAsync();
        
        foreach (var sensor in sensors)
        {
            if (!_sensorProfiles.TryGetValue(sensor.Type, out var profile))
                continue;
            
            var hourOfDay = now.Hour + now.Minute / 60.0;
            var baseValue = (double)profile.BaseValue;
            var amplitude = (double)profile.Amplitude;
            var sinusoidalValue = baseValue + amplitude * Math.Sin(2 * Math.PI * (hourOfDay + profile.PhaseOffset) / profile.PeriodHours);
            var noise = _random.NextGaussian(0, (double)profile.NoiseStdDev);
            var finalValue = Math.Max((double)profile.MinValue, Math.Min((double)profile.MaxValue, sinusoidalValue + noise));
            
            readings.Add(new SensorReading
            {
                SensorId = sensor.Id,
                Value = (decimal)finalValue,
                Timestamp = now,
                Quality = "Good"
            });
        }
        
        return readings;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _collectionTimer?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}

// Extension for Gaussian random numbers
public static class RandomExtensions
{
    public static double NextGaussian(this Random random, double mean, double stdDev)
    {
        var u1 = 1.0 - random.NextDouble();
        var u2 = 1.0 - random.NextDouble();
        var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        return mean + stdDev * randStdNormal;
    }
}

public class SensorProfile
{
    public decimal BaseValue { get; set; }
    public decimal Amplitude { get; set; }
    public double PeriodHours { get; set; }
    public double PhaseOffset { get; set; }
    public decimal NoiseStdDev { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }
}
