using GreenhouseGuardian.Application.Interfaces;
using GreenhouseGuardian.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenhouseGuardian.Infrastructure.Services;

public class AlertService : IAlertService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public AlertService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<Alert>> GetAllAlertsAsync(bool? isAcknowledged = null)
    {
        var query = _context.Alerts
            .Include(a => a.Sensor)
            .Include(a => a.Actuator)
            .OrderByDescending(a => a.CreatedAt)
            .AsQueryable();

        if (isAcknowledged.HasValue)
            query = query.Where(a => a.IsAcknowledged == isAcknowledged.Value);

        return await query.Take(500).ToListAsync();
    }

    public async Task<Alert> AcknowledgeAlertAsync(long alertId, string userId)
    {
        var alert = await _context.Alerts.FindAsync(alertId)
            ?? throw new InvalidOperationException($"Alert {alertId} not found");

        alert.IsAcknowledged = true;
        alert.AcknowledgedAt = DateTime.UtcNow;
        alert.AcknowledgedBy = userId;

        await _context.SaveChangesAsync();
        return alert;
    }

    public async Task CreateAlertAsync(Alert alert)
    {
        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync();

        // Send notifications for critical alerts
        if (alert.Severity == AlertSeverity.Critical)
        {
            var channels = await _context.NotificationChannels
                .Where(c => c.IsActive && c.ChannelType == "Telegram")
                .ToListAsync();

            foreach (var channel in channels)
            {
                try
                {
                    await _notificationService.SendNotificationAsync(alert.Id, channel.ChannelType, channel.ChannelAddress);
                }
                catch (Exception ex)
                {
                    // Log but don't fail the alert creation
                    Console.WriteLine($"Failed to send notification: {ex.Message}");
                }
            }
        }
    }
}
