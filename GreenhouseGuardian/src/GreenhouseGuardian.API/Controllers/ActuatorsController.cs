using GreenhouseGuardian.Application.Interfaces;
using GreenhouseGuardian.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenhouseGuardian.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ActuatorsController : ControllerBase
{
    private readonly IActuatorService _actuatorService;
    private readonly IAuditService _auditService;
    private readonly ILogger<ActuatorsController> _logger;

    public ActuatorsController(
        IActuatorService actuatorService,
        IAuditService auditService,
        ILogger<ActuatorsController> logger)
    {
        _actuatorService = actuatorService;
        _auditService = auditService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var actuators = await _actuatorService.GetAllActuatorsAsync();
        return Ok(actuators.Select(a => new
        {
            a.Id,
            a.Name,
            a.Code,
            Type = a.Type.ToString(),
            a.ZoneId,
            ZoneName = a.Zone?.Name,
            State = a.State.ToString(),
            a.IsAutoMode,
            a.IsActive,
            a.LastCommandAt,
            a.LastCommandBy
        }));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var actuator = await _actuatorService.GetActuatorByIdAsync(id);
        if (actuator == null) return NotFound();
        return Ok(actuator);
    }

    [HttpPost("{id:int}/command")]
    [Authorize(Roles = "Administrator,Agronomist,Operator")]
    public async Task<IActionResult> ExecuteCommand(int id, [FromBody] ActuatorCommandDto command)
    {
        var userId = User.FindFirst("sub")?.Value ?? "unknown";
        
        var actuator = await _actuatorService.ExecuteCommandAsync(id, command.Action.ToLower(), userId);
        
        // Log to audit
        await _auditService.LogActionAsync(
            userId,
            $"ActuatorCommand:{command.Action}",
            "Actuator",
            id,
            null,
            $"Action={command.Action}, Reason={command.Reason}",
            GetIpAddress());

        _logger.LogInformation("Actuator {ActuatorId} command {Action} executed by user {UserId}", id, command.Action, userId);
        
        return Ok(new { Success = true, ActuatorId = id, NewState = actuator.State.ToString() });
    }

    [HttpGet("{id:int}/logs")]
    public async Task<IActionResult> GetLogs(int id, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var logs = await _actuatorService.GetActuatorLogsAsync(id, from, to);
        return Ok(logs.Select(l => new
        {
            l.Id,
            l.ActuatorId,
            PreviousState = l.PreviousState.ToString(),
            NewState = l.NewState.ToString(),
            l.TriggeredBy,
            l.Reason,
            l.ExecutedAt
        }));
    }

    private string GetIpAddress()
    {
        if (HttpContext.Connection.RemoteIpAddress != null)
        {
            return HttpContext.Connection.RemoteIpAddress.ToString();
        }
        return "unknown";
    }
}
