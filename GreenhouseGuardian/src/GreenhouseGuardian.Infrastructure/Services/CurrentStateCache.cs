using GreenhouseGuardian.Application.Interfaces;
using GreenhouseGuardian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace GreenhouseGuardian.Infrastructure.Services;

public class CurrentStateCache : ICurrentStateCache
{
    private readonly IDatabase? _redis;
    private readonly string _sensorKeyPrefix = "sensor:current:";
    private readonly string _actuatorKeyPrefix = "actuator:state:";
    private readonly ConcurrentDictionary<int, decimal> _sensorCache = new();
    private readonly ConcurrentDictionary<int, ActuatorState> _actuatorCache = new();

    public CurrentStateCache(IConnectionMultiplexer? redis = null)
    {
        _redis = redis?.GetDatabase();
    }

    public async Task<IDictionary<int, decimal>> GetAllSensorCurrentValuesAsync()
    {
        if (_redis != null)
        {
            var result = new Dictionary<int, decimal>();
            
            // Get all keys with sensor prefix
            var server = _redis.Multiplexer.GetServer(_redis.Multiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: $"{_sensorKeyPrefix}*");
            
            foreach (var key in keys)
            {
                var value = await _redis.StringGetAsync(key);
                if (value.HasValue && int.TryParse(key.ToString().Replace(_sensorKeyPrefix, ""), out var sensorId))
                {
                    result[sensorId] = decimal.Parse(value!);
                }
            }
            
            return result;
        }
        
        // Fallback to in-memory cache
        return _sensorCache.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public async Task<decimal?> GetSensorCurrentValueAsync(int sensorId)
    {
        if (_redis != null)
        {
            var value = await _redis.StringGetAsync($"{_sensorKeyPrefix}{sensorId}");
            return value.HasValue ? decimal.Parse(value!) : null;
        }
        
        // Fallback to in-memory cache
        return _sensorCache.TryGetValue(sensorId, out var val) ? val : null;
    }

    public async Task SetSensorCurrentValueAsync(int sensorId, decimal value)
    {
        if (_redis != null)
        {
            await _redis.StringSetAsync($"{_sensorKeyPrefix}{sensorId}", value.ToString());
        }
        
        // Also update in-memory cache
        _sensorCache[sensorId] = value;
    }

    public async Task<Dictionary<int, ActuatorState>> GetAllActuatorStatesAsync()
    {
        if (_redis != null)
        {
            var result = new Dictionary<int, ActuatorState>();
            
            var server = _redis.Multiplexer.GetServer(_redis.Multiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: $"{_actuatorKeyPrefix}*");
            
            foreach (var key in keys)
            {
                var value = await _redis.StringGetAsync(key);
                if (value.HasValue && int.TryParse(key.ToString().Replace(_actuatorKeyPrefix, ""), out var actuatorId))
                {
                    result[actuatorId] = Enum.Parse<ActuatorState>(value!);
                }
            }
            
            return result;
        }
        
        // Fallback to in-memory cache
        return _actuatorCache.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public async Task<ActuatorState?> GetActuatorStateAsync(int actuatorId)
    {
        if (_redis != null)
        {
            var value = await _redis.StringGetAsync($"{_actuatorKeyPrefix}{actuatorId}");
            return value.HasValue ? Enum.Parse<ActuatorState>(value!) : null;
        }
        
        // Fallback to in-memory cache
        return _actuatorCache.TryGetValue(actuatorId, out var state) ? state : null;
    }

    public async Task SetActuatorStateAsync(int actuatorId, ActuatorState state)
    {
        if (_redis != null)
        {
            await _redis.StringSetAsync($"{_actuatorKeyPrefix}{actuatorId}", state.ToString());
        }
        
        // Also update in-memory cache
        _actuatorCache[actuatorId] = state;
    }
}
