using GreenhouseGuardian.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenhouseGuardian.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SensorsController : ControllerBase
{
    private readonly ISensorService _sensorService;
    private readonly ILogger<SensorsController> _logger;

    public SensorsController(ISensorService sensorService, ILogger<SensorsController> logger)
    {
        _sensorService = sensorService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var sensors = await _sensorService.GetAllSensorsAsync();
        return Ok(sensors.Select(s => new
        {
            s.Id,
            s.Name,
            s.Code,
            Type = s.Type.ToString(),
            s.ZoneId,
            ZoneName = s.Zone?.Name,
            s.WarningThresholdLow,
            s.WarningThresholdHigh,
            s.CriticalThresholdLow,
            s.CriticalThresholdHigh,
            s.Unit,
            s.IsActive,
            s.CreatedAt
        }));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var sensor = await _sensorService.GetSensorByIdAsync(id);
        if (sensor == null) return NotFound();
        return Ok(sensor);
    }

    [HttpGet("{id:int}/history")]
    public async Task<IActionResult> GetHistory(int id, [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] string? interval = null)
    {
        var readings = await _sensorService.GetSensorHistoryAsync(id, from, to, interval);
        return Ok(readings.Select(r => new
        {
            r.Id,
            r.SensorId,
            r.Value,
            r.Timestamp,
            r.Quality
        }));
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,Agronomist")]
    public async Task<IActionResult> Create([FromBody] CreateSensorRequest request)
    {
        var sensor = new Domain.Entities.Sensor
        {
            Name = request.Name,
            Code = request.Code,
            Type = Enum.Parse<Domain.Entities.SensorType>(request.Type),
            ZoneId = request.ZoneId,
            WarningThresholdLow = request.WarningThresholdLow,
            WarningThresholdHigh = request.WarningThresholdHigh,
            CriticalThresholdLow = request.CriticalThresholdLow,
            CriticalThresholdHigh = request.CriticalThresholdHigh,
            Unit = request.Unit,
            IsActive = true
        };

        var created = await _sensorService.CreateSensorAsync(sensor);
        _logger.LogInformation("Sensor created by user {UserId}", User.FindFirst("sub")?.Value);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrator,Agronomist")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSensorRequest request)
    {
        var sensor = await _sensorService.GetSensorByIdAsync(id);
        if (sensor == null) return NotFound();

        sensor.Name = request.Name;
        sensor.ZoneId = request.ZoneId;
        sensor.WarningThresholdLow = request.WarningThresholdLow;
        sensor.WarningThresholdHigh = request.WarningThresholdHigh;
        sensor.CriticalThresholdLow = request.CriticalThresholdLow;
        sensor.CriticalThresholdHigh = request.CriticalThresholdHigh;
        sensor.IsActive = request.IsActive;

        var updated = await _sensorService.UpdateSensorAsync(sensor);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(int id)
    {
        await _sensorService.DeleteSensorAsync(id);
        return NoContent();
    }
}

public class CreateSensorRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? ZoneId { get; set; }
    public decimal WarningThresholdLow { get; set; }
    public decimal WarningThresholdHigh { get; set; }
    public decimal CriticalThresholdLow { get; set; }
    public decimal CriticalThresholdHigh { get; set; }
    public string Unit { get; set; } = string.Empty;
}

public class UpdateSensorRequest
{
    public string Name { get; set; } = string.Empty;
    public string? ZoneId { get; set; }
    public decimal WarningThresholdLow { get; set; }
    public decimal WarningThresholdHigh { get; set; }
    public decimal CriticalThresholdLow { get; set; }
    public decimal CriticalThresholdHigh { get; set; }
    public bool IsActive { get; set; }
}
