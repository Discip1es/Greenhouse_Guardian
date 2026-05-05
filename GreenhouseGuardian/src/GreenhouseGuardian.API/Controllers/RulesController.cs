using GreenhouseGuardian.Application.Interfaces;
using GreenhouseGuardian.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenhouseGuardian.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class RulesController : ControllerBase
{
    private readonly IAutomationRuleService _ruleService;
    private readonly ILogger<RulesController> _logger;

    public RulesController(IAutomationRuleService ruleService, ILogger<RulesController> logger)
    {
        _ruleService = ruleService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var rules = await _ruleService.GetAllRulesAsync();
        return Ok(rules.Select(r => new
        {
            r.Id,
            r.Name,
            r.Description,
            r.IsActive,
            r.IsPaused,
            r.SensorId,
            SensorName = r.Sensor?.Name,
            r.ConditionOperator,
            r.ThresholdValue,
            r.DurationMinutes,
            r.ActuatorId,
            ActuatorName = r.Actuator?.Name,
            r.Action,
            r.LastTriggeredAt,
            r.CreatedAt
        }));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var rule = await _ruleService.GetRuleByIdAsync(id);
        if (rule == null) return NotFound();
        return Ok(rule);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,Agronomist")]
    public async Task<IActionResult> Create([FromBody] CreateRuleRequest request)
    {
        var rule = new AutomationRule
        {
            Name = request.Name,
            Description = request.Description,
            SensorId = request.SensorId,
            ConditionOperator = request.ConditionOperator,
            ThresholdValue = request.ThresholdValue,
            DurationMinutes = request.DurationMinutes,
            ActuatorId = request.ActuatorId,
            Action = request.Action,
            IsActive = true,
            IsPaused = false
        };

        var created = await _ruleService.CreateRuleAsync(rule);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrator,Agronomist")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRuleRequest request)
    {
        var rule = await _ruleService.GetRuleByIdAsync(id);
        if (rule == null) return NotFound();

        rule.Name = request.Name;
        rule.Description = request.Description;
        rule.IsActive = request.IsActive;
        rule.IsPaused = request.IsPaused;
        rule.ConditionOperator = request.ConditionOperator;
        rule.ThresholdValue = request.ThresholdValue;
        rule.DurationMinutes = request.DurationMinutes;
        rule.Action = request.Action;

        var updated = await _ruleService.UpdateRuleAsync(rule);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(int id)
    {
        await _ruleService.DeleteRuleAsync(id);
        return NoContent();
    }

    [HttpPost("{id:int}/pause")]
    [Authorize(Roles = "Administrator,Agronomist")]
    public async Task<IActionResult> Pause(int id)
    {
        await _ruleService.PauseRuleAsync(id);
        return Ok(new { Success = true });
    }

    [HttpPost("{id:int}/resume")]
    [Authorize(Roles = "Administrator,Agronomist")]
    public async Task<IActionResult> Resume(int id)
    {
        await _ruleService.ResumeRuleAsync(id);
        return Ok(new { Success = true });
    }
}

public class CreateRuleRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? SensorId { get; set; }
    public string ConditionOperator { get; set; } = string.Empty;
    public decimal ThresholdValue { get; set; }
    public int DurationMinutes { get; set; }
    public int? ActuatorId { get; set; }
    public string Action { get; set; } = string.Empty;
}

public class UpdateRuleRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool IsPaused { get; set; }
    public string ConditionOperator { get; set; } = string.Empty;
    public decimal ThresholdValue { get; set; }
    public int DurationMinutes { get; set; }
    public string Action { get; set; } = string.Empty;
}
