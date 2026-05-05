using GreenhouseGuardian.Application.Interfaces;
using GreenhouseGuardian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenhouseGuardian.Infrastructure.Services;

public class AutomationRuleService : IAutomationRuleService
{
    private readonly ApplicationDbContext _context;
    private readonly IActuatorService _actuatorService;
    private readonly ICurrentStateCache _cache;
    private readonly ILogger<AutomationRuleService> _logger;

    public AutomationRuleService(
        ApplicationDbContext context,
        IActuatorService actuatorService,
        ICurrentStateCache cache,
        ILogger<AutomationRuleService> logger)
    {
        _context = context;
        _actuatorService = actuatorService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<AutomationRule>> GetAllRulesAsync()
    {
        return await _context.AutomationRules
            .Include(r => r.Zone)
            .Include(r => r.Sensor)
            .Include(r => r.Actuator)
            .ToListAsync();
    }

    public async Task<AutomationRule?> GetRuleByIdAsync(int id)
    {
        return await _context.AutomationRules
            .Include(r => r.Zone)
            .Include(r => r.Sensor)
            .Include(r => r.Actuator)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<AutomationRule> CreateRuleAsync(AutomationRule rule)
    {
        _context.AutomationRules.Add(rule);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Automation rule {rule.Id} created for zone {rule.ZoneId}");
        return rule;
    }

    public async Task<AutomationRule> UpdateRuleAsync(AutomationRule rule)
    {
        var existing = await _context.AutomationRules.FindAsync(rule.Id);
        if (existing == null)
            throw new KeyNotFoundException($"Rule with ID {rule.Id} not found.");

        existing.ConditionType = rule.ConditionType;
        existing.ThresholdValue = rule.ThresholdValue;
        existing.ActuatorAction = rule.ActuatorAction;
        existing.IsEnabled = rule.IsEnabled;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation($"Automation rule {rule.Id} updated");
        return existing;
    }

    public async Task DeleteRuleAsync(int id)
    {
        var rule = await _context.AutomationRules.FindAsync(id);
        if (rule != null)
        {
            _context.AutomationRules.Remove(rule);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Automation rule {id} deleted");
        }
    }

    public async Task PauseRuleAsync(int id)
    {
        var rule = await _context.AutomationRules.FindAsync(id);
        if (rule != null)
        {
            rule.IsEnabled = false;
            rule.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Automation rule {id} paused");
        }
    }

    public async Task ResumeRuleAsync(int id)
    {
        var rule = await _context.AutomationRules.FindAsync(id);
        if (rule != null)
        {
            rule.IsEnabled = true;
            rule.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Automation rule {id} resumed");
        }
    }

    public async Task EvaluateRulesAsync()
    {
        var activeRules = await _context.AutomationRules
            .Include(r => r.Sensor)
            .Include(r => r.Actuator)
            .Where(r => r.IsEnabled)
            .ToListAsync();

        var currentValues = await _cache.GetAllSensorCurrentValuesAsync();

        foreach (var rule in activeRules)
        {
            if (!currentValues.TryGetValue(rule.SensorId, out var currentValue))
                continue;

            bool conditionMet = rule.ConditionType switch
            {
                RuleConditionType.GreaterThan => currentValue > rule.ThresholdValue,
                RuleConditionType.LessThan => currentValue < rule.ThresholdValue,
                RuleConditionType.Equals => currentValue == rule.ThresholdValue,
                RuleConditionType.NotEquals => currentValue != rule.ThresholdValue,
                RuleConditionType.GreaterThanOrEqual => currentValue >= rule.ThresholdValue,
                RuleConditionType.LessThanOrEqual => currentValue <= rule.ThresholdValue,
                _ => false
            };

            if (conditionMet)
            {
                try
                {
                    await _actuatorService.ExecuteCommandAsync(rule.ActuatorId, rule.ActuatorAction);
                    
                    var executionLog = new RuleExecutionLog
                    {
                        RuleId = rule.Id,
                        ExecutedAt = DateTime.UtcNow,
                        SensorValue = currentValue,
                        TriggeredAction = rule.ActuatorAction,
                        Success = true
                    };
                    _context.RuleExecutionLogs.Add(executionLog);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation($"Rule {rule.Id} triggered: Actuator {rule.ActuatorId} executed {rule.ActuatorAction}");
                }
                catch (Exception ex)
                {
                    var executionLog = new RuleExecutionLog
                    {
                        RuleId = rule.Id,
                        ExecutedAt = DateTime.UtcNow,
                        SensorValue = currentValue,
                        TriggeredAction = rule.ActuatorAction,
                        Success = false,
                        ErrorMessage = ex.Message
                    };
                    _context.RuleExecutionLogs.Add(executionLog);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogError(ex, $"Rule {rule.Id} failed to execute");
                }
            }
        }
    }
}
