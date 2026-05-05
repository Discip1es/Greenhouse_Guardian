using GreenhouseGuardian.Application.Interfaces;
using GreenhouseGuardian.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GreenhouseGuardian.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;

    public AuditService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogActionAsync(string userId, string action, string entityType, int? entityId, string? oldValues, string? newValues, string ipAddress)
    {
        var auditLog = new Domain.Entities.AuditLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Domain.Entities.AuditLog>> GetAuditLogsAsync(string? userId = null, string? action = null, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.AuditLogs
            .OrderByDescending(l => l.CreatedAt)
            .AsQueryable();

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(l => l.UserId == userId);
        if (!string.IsNullOrEmpty(action))
            query = query.Where(l => l.Action.Contains(action));
        if (from.HasValue)
            query = query.Where(l => l.CreatedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(l => l.CreatedAt <= to.Value);

        return await query.Take(1000).ToListAsync();
    }
}
