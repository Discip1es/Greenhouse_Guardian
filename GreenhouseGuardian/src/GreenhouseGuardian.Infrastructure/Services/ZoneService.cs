using GreenhouseGuardian.Application.Interfaces;
using GreenhouseGuardian.Domain.Entities;
using GreenhouseGuardian.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenhouseGuardian.Infrastructure.Services;

public class ZoneService : IZoneService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ZoneService> _logger;

    public ZoneService(ApplicationDbContext context, ILogger<ZoneService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Zone>> GetAllZonesAsync()
    {
        return await _context.Zones
            .Include(z => z.PlantProfile)
            .Include(z => z.Sensors)
            .Include(z => z.Actuators)
            .OrderBy(z => z.Name)
            .ToListAsync();
    }

    public async Task<Zone?> GetZoneByIdAsync(string id)
    {
        return await _context.Zones
            .Include(z => z.PlantProfile)
            .Include(z => z.Sensors)
            .Include(z => z.Actuators)
            .FirstOrDefaultAsync(z => z.Id == id);
    }

    public async Task<Zone> CreateZoneAsync(Zone zone)
    {
        _context.Zones.Add(zone);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Zone {ZoneName} created", zone.Name);
        return zone;
    }

    public async Task<Zone> UpdateZoneAsync(Zone zone)
    {
        var existing = await _context.Zones.FindAsync(zone.Id);
        if (existing == null) throw new InvalidOperationException($"Zone {zone.Id} not found");

        existing.Name = zone.Name;
        existing.Description = zone.Description;
        existing.PlantProfileId = zone.PlantProfileId;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Zone {ZoneName} updated", existing.Name);
        return existing;
    }

    public async Task DeleteZoneAsync(string id)
    {
        var zone = await _context.Zones.FindAsync(id);
        if (zone != null)
        {
            _context.Zones.Remove(zone);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Zone {ZoneName} deleted", zone.Name);
        }
    }
}
