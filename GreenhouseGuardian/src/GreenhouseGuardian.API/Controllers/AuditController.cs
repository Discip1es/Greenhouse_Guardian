using GreenhouseGuardian.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenhouseGuardian.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Administrator")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;
    private readonly ILogger<AuditController> _logger;

    public AuditController(IAuditService auditService, ILogger<AuditController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs(
        [FromQuery] string? userId,
        [FromQuery] string? action,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var logs = await _auditService.GetAuditLogsAsync(userId, action, from, to);
        return Ok(logs.Select(l => new
        {
            l.Id,
            l.UserId,
            Action = l.Action,
            EntityType = l.EntityType,
            l.EntityId,
            l.OldValues,
            l.NewValues,
            l.IpAddress,
            l.CreatedAt
        }));
    }
}
