using GreenhouseGuardian.Application.Interfaces;
using GreenhouseGuardian.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenhouseGuardian.Infrastructure.Services;

public class ActuatorService : IActuatorService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentStateCache _cache;

    public ActuatorService(ApplicationDbContext context, ICurrentStateCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<IEnumerable<Actuator>> GetAllActuatorsAsync()
    {
        return await _context.Actuators
            .Include(a => a.Zone)
            .OrderBy(a => a.Code)
            .ToListAsync();
    }

    public async Task<Actuator?> GetActuatorByIdAsync(int id)
    {
        return await _context.Actuators
            .Include(a => a.Zone)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Actuator> ExecuteCommandAsync(int actuatorId, string action, string? userId = null)
    {
        var actuator = await _context.Actuators.FindAsync(actuatorId)
            ?? throw new InvalidOperationException($"Actuator {actuatorId} not found");

        var previousState = actuator.State;
        var newState = action.ToLower() switch
        {
            "on" => ActuatorState.On,
            "off" => ActuatorState.Off,
            _ => throw new ArgumentException($"Invalid action: {action}")
        };

        actuator.State = newState;
        actuator.LastCommandAt = DateTime.UtcNow;
        actuator.LastCommandBy = userId;

        // Log the command
        var log = new ActuatorLog
        {
            ActuatorId = actuatorId,
            PreviousState = previousState,
            NewState = newState,
            TriggeredBy = userId,
            ExecutedAt = DateTime.UtcNow
        };
        _context.ActuatorLogs.Add(log);

        // Update cache
        await _cache.SetActuatorStateAsync(actuatorId, newState);

        await _context.SaveChangesAsync();
        return actuator;
    }

    public async Task<IEnumerable<ActuatorLog>> GetActuatorLogsAsync(int actuatorId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.ActuatorLogs
            .Where(l => l.ActuatorId == actuatorId)
            .OrderByDescending(l => l.ExecutedAt);

        if (from.HasValue)
            query = query.Where(l => l.ExecutedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(l => l.ExecutedAt <= to.Value);

        return await query.Take(1000).ToListAsync();
    }
}
