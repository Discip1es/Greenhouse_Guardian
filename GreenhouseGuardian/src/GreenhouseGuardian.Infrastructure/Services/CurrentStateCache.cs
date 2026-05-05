using GreenhouseGuardian.Application.Interfaces;
using GreenhouseGuardian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace GreenhouseGuardian.Infrastructure.Services;

public class CurrentStateCache : ICurrentStateCache
{
    private readonly IDatabase _redis;
    private readonly string _sensorKeyPrefix = "sensor:current:";
    private readonly string _actuatorKeyPrefix = "actuator:state:";

    public CurrentStateCache(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
    }

    public async Task<IDictionary<int, decimal>> GetAllSensorCurrentValuesAsync()
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

    public async Task<decimal?> GetSensorCurrentValueAsync(int sensorId)
    {
        var value = await _redis.StringGetAsync($"{_sensorKeyPrefix}{sensorId}");
        return value.HasValue ? decimal.Parse(value!) : null;
    }

    public async Task SetSensorCurrentValueAsync(int sensorId, decimal value)
    {
        await _redis.StringSetAsync($"{_sensorKeyPrefix}{sensorId}", value.ToString());
    }

    public async Task<Dictionary<int, ActuatorState>> GetAllActuatorStatesAsync()
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

    public async Task<ActuatorState?> GetActuatorStateAsync(int actuatorId)
    {
        var value = await _redis.StringGetAsync($"{_actuatorKeyPrefix}{actuatorId}");
        return value.HasValue ? Enum.Parse<ActuatorState>(value!) : null;
    }

    public async Task SetActuatorStateAsync(int actuatorId, ActuatorState state)
    {
        await _redis.StringSetAsync($"{_actuatorKeyPrefix}{actuatorId}", state.ToString());
    }
}
