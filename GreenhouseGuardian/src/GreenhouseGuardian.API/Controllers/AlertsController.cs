using GreenhouseGuardian.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenhouseGuardian.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _alertService;
    private readonly ILogger<AlertsController> _logger;

    public AlertsController(IAlertService alertService, ILogger<AlertsController> logger)
    {
        _alertService = alertService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isAcknowledged = null)
    {
        var alerts = await _alertService.GetAllAlertsAsync(isAcknowledged);
        return Ok(alerts.Select(a => new
        {
            a.Id,
            a.SensorId,
            SensorName = a.Sensor?.Name,
            a.ActuatorId,
            ActuatorName = a.Actuator?.Name,
            Severity = a.Severity.ToString(),
            a.Title,
            a.Message,
            a.IsAcknowledged,
            a.AcknowledgedAt,
            a.AcknowledgedBy,
            a.CreatedAt
        }));
    }

    [HttpPost("{id:long}/acknowledge")]
    [Authorize(Roles = "Administrator,Agronomist,Operator")]
    public async Task<IActionResult> Acknowledge(long id)
    {
        var userId = User.FindFirst("sub")?.Value ?? "unknown";
        var alert = await _alertService.AcknowledgeAlertAsync(id, userId);
        return Ok(new 
        { 
            Success = true, 
            AlertId = id,
            AcknowledgedAt = alert.AcknowledgedAt,
            AcknowledgedBy = alert.AcknowledgedBy
        });
    }
}
