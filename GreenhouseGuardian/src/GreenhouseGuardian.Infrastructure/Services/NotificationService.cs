using GreenhouseGuardian.Application.Interfaces;
using GreenhouseGuardian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net.Http.Json;

namespace GreenhouseGuardian.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        ApplicationDbContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task SendNotificationAsync(int alertId, string channelType, string channelAddress)
    {
        var alert = await _context.Alerts.FindAsync(alertId);
        if (alert == null)
        {
            _logger.LogWarning($"Alert with ID {alertId} not found.");
            return;
        }

        var notificationLog = new NotificationLog
        {
            AlertId = alertId,
            ChannelType = channelType,
            ChannelAddress = channelAddress,
            SentAt = DateTime.UtcNow,
            Status = "Pending"
        };

        try
        {
            switch (channelType.ToLower())
            {
                case "email":
                    await SendEmailNotificationAsync(channelAddress, alert);
                    break;
                case "telegram":
                    await SendTelegramNotificationAsync(channelAddress, alert);
                    break;
                case "webhook":
                    await SendWebhookNotificationAsync(channelAddress, alert);
                    break;
                default:
                    throw new NotSupportedException($"Notification channel '{channelType}' is not supported.");
            }

            notificationLog.Status = "Sent";
            _logger.LogInformation($"Notification sent successfully via {channelType} to {channelAddress} for alert {alertId}.");
        }
        catch (Exception ex)
        {
            notificationLog.Status = "Failed";
            notificationLog.ErrorMessage = ex.Message;
            _logger.LogError(ex, $"Failed to send notification via {channelType} to {channelAddress} for alert {alertId}.");
        }
        finally
        {
            _context.NotificationLogs.Add(notificationLog);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SendEmailNotificationAsync(string email, Alert alert)
    {
        // В реальном приложении здесь будет отправка через SMTP или сервис вроде SendGrid
        // Для демонстрации просто логируем
        _logger.LogInformation($"[EMAIL] To: {email}, Subject: [Greenhouse Guardian] Alert: {alert.Severity}", 
            $"Message: Sensor {alert.SensorId} - Value: {alert.Value}, Threshold: {alert.ThresholdValue} at {alert.CreatedAt}");
        
        await Task.Delay(100); // Имитация задержки отправки
    }

    private async Task SendTelegramNotificationAsync(string chatId, Alert alert)
    {
        // В реальном приложении здесь будет вызов Telegram Bot API
        var message = $"""
            🚨 *Greenhouse Guardian Alert*
            
            Severity: *{alert.Severity}*
            Sensor ID: {alert.SensorId}
            Current Value: {alert.Value}
            Threshold: {alert.ThresholdValue}
            Zone: {alert.ZoneId}
            Time: {alert.CreatedAt:yyyy-MM-dd HH:mm:ss}
            
            {alert.Message}
            """;

        _logger.LogInformation($"[TELEGRAM] Chat: {chatId}, Message: {message}");
        
        // Пример реального вызова (раскомментировать и настроить токен):
        /*
        var client = _httpClientFactory.CreateClient();
        var token = "YOUR_BOT_TOKEN";
        var url = $"https://api.telegram.org/bot{token}/sendMessage";
        
        var content = new JsonContent(new
        {
            chat_id = chatId,
            text = message,
            parse_mode = "Markdown"
        });
        
        var response = await client.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
        */
        
        await Task.Delay(100); // Имитация задержки
    }

    private async Task SendWebhookNotificationAsync(string webhookUrl, Alert alert)
    {
        var client = _httpClientFactory.CreateClient();
        
        var payload = new
        {
            alertId = alert.Id,
            severity = alert.Severity.ToString(),
            sensorId = alert.SensorId,
            value = alert.Value,
            thresholdValue = alert.ThresholdValue,
            zoneId = alert.ZoneId,
            message = alert.Message,
            createdAt = alert.CreatedAt
        };

        var content = JsonContent.Create(payload);
        var response = await client.PostAsync(webhookUrl, content);
        response.EnsureSuccessStatusCode();
        
        _logger.LogInformation($"[WEBHOOK] Sent to {webhookUrl} for alert {alertId}");
    }

    public async Task<IEnumerable<NotificationLog>> GetNotificationHistoryAsync(int? alertId = null)
    {
        IQueryable<NotificationLog> query = _context.NotificationLogs
            .Include(nl => nl.Alert)
            .OrderByDescending(nl => nl.SentAt);

        if (alertId.HasValue)
        {
            query = query.Where(nl => nl.AlertId == alertId.Value);
        }

        return await query.ToListAsync();
    }
}
