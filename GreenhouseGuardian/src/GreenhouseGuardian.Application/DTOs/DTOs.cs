using GreenhouseGuardian.Domain.Entities;

namespace GreenhouseGuardian.Application.DTOs;

public class SensorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public SensorType Type { get; set; }
    public string? ZoneId { get; set; }
    public string? ZoneName { get; set; }
    public decimal WarningThresholdLow { get; set; }
    public decimal WarningThresholdHigh { get; set; }
    public decimal CriticalThresholdLow { get; set; }
    public decimal CriticalThresholdHigh { get; set; }
    public string Unit { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public decimal? CurrentValue { get; set; }
    public DateTime? LastReadingAt { get; set; }
}

public class SensorReadingDto
{
    public long Id { get; set; }
    public int SensorId { get; set; }
    public string SensorName { get; set; } = string.Empty;
    public SensorType Type { get; set; }
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Quality { get; set; }
}

public class ActuatorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public ActuatorType Type { get; set; }
    public string? ZoneId { get; set; }
    public string? ZoneName { get; set; }
    public ActuatorState State { get; set; }
    public bool IsAutoMode { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastCommandAt { get; set; }
    public string? LastCommandBy { get; set; }
}

public class ActuatorCommandDto
{
    public string Action { get; set; } = string.Empty;  // "on", "off"
    public string? Reason { get; set; }
}

public class ZoneDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PlantProfileId { get; set; }
    public string? PlantProfileName { get; set; }
    public int SensorCount { get; set; }
    public int ActuatorCount { get; set; }
}

public class PlantProfileDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CultureName { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Эталонные диапазоны
    public decimal TempDayMin { get; set; }
    public decimal TempDayMax { get; set; }
    public decimal TempNightMin { get; set; }
    public decimal TempNightMax { get; set; }
    public decimal HumidityMin { get; set; }
    public decimal HumidityMax { get; set; }
    public decimal LightMin { get; set; }
    public decimal LightMax { get; set; }
    public decimal CO2Min { get; set; }
    public decimal CO2Max { get; set; }
    public decimal PHMin { get; set; }
    public decimal PHMax { get; set; }
    public decimal ECMin { get; set; }
    public decimal ECMax { get; set; }
}

public class AutomationRuleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool IsPaused { get; set; }
    
    public int? SensorId { get; set; }
    public string? SensorName { get; set; }
    public string ConditionOperator { get; set; } = string.Empty;
    public decimal ThresholdValue { get; set; }
    public int DurationMinutes { get; set; }
    
    public int? ActuatorId { get; set; }
    public string? ActuatorName { get; set; }
    public string Action { get; set; } = string.Empty;
    
    public DateTime? LastTriggeredAt { get; set; }
}

public class AlertDto
{
    public long Id { get; set; }
    public int? SensorId { get; set; }
    public string? SensorName { get; set; }
    public int? ActuatorId { get; set; }
    public string? ActuatorName { get; set; }
    public AlertSeverity Severity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsAcknowledged { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public string? AcknowledgedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class AuditLogDto
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class TelemetryUpdateDto
{
    public List<SensorTelemetryDto> Sensors { get; set; } = new();
    public List<ActuatorTelemetryDto> Actuators { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class SensorTelemetryDto
{
    public int SensorId { get; set; }
    public string Code { get; set; } = string.Empty;
    public SensorType Type { get; set; }
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;  // "Normal", "Warning", "Critical"
}

public class ActuatorTelemetryDto
{
    public int ActuatorId { get; set; }
    public string Code { get; set; } = string.Empty;
    public ActuatorState State { get; set; }
}

public class NotificationChannelDto
{
    public int Id { get; set; }
    public string ChannelType { get; set; } = string.Empty;
    public string ChannelAddress { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class RecommendationDto
{
    public string ZoneName { get; set; } = string.Empty;
    public string CultureName { get; set; } = string.Empty;
    public List<string> Recommendations { get; set; } = new();
}
