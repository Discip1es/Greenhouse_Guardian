namespace GreenhouseGuardian.Domain.Entities;

public class Sensor
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public SensorType Type { get; set; }
    public string? ZoneId { get; set; }
    public Zone? Zone { get; set; }
    public decimal WarningThresholdLow { get; set; }
    public decimal WarningThresholdHigh { get; set; }
    public decimal CriticalThresholdLow { get; set; }
    public decimal CriticalThresholdHigh { get; set; }
    public string Unit { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Навигационные свойства
    public ICollection<SensorReading> Readings { get; set; } = new List<SensorReading>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}

public class SensorReading
{
    public long Id { get; set; }
    public int SensorId { get; set; }
    public Sensor? Sensor { get; set; }
    public decimal Value { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Quality { get; set; }  // "Good", "Suspect", "Bad"
}

public class Actuator
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public ActuatorType Type { get; set; }
    public string? ZoneId { get; set; }
    public Zone? Zone { get; set; }
    public ActuatorState State { get; set; } = ActuatorState.Off;
    public bool IsAutoMode { get; set; } = false;
    public DateTime? LastCommandAt { get; set; }
    public string? LastCommandBy { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Навигационные свойства
    public ICollection<ActuatorLog> CommandLogs { get; set; } = new List<ActuatorLog>();
}

public class ActuatorLog
{
    public long Id { get; set; }
    public int ActuatorId { get; set; }
    public Actuator? Actuator { get; set; }
    public ActuatorState PreviousState { get; set; }
    public ActuatorState NewState { get; set; }
    public string? TriggeredBy { get; set; }  // User ID или Rule ID
    public string? Reason { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}

public class Zone
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PlantProfileId { get; set; }
    public PlantProfile? PlantProfile { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Навигационные свойства
    public ICollection<Sensor> Sensors { get; set; } = new List<Sensor>();
    public ICollection<Actuator> Actuators { get; set; } = new List<Actuator>();
}

public class PlantProfile
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CultureName { get; set; } = string.Empty;  // Например, "Томаты"
    
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
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Навигационные свойства
    public ICollection<Zone> Zones { get; set; } = new List<Zone>();
}

public class AutomationRule
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPaused { get; set; } = false;
    
    // Условие: if (параметр X >/< порога) на протяжении N минут
    public int? SensorId { get; set; }
    public Sensor? Sensor { get; set; }
    public string ConditionOperator { get; set; } = string.Empty;  // "gt", "lt", "gte", "lte", "eq"
    public decimal ThresholdValue { get; set; }
    public int DurationMinutes { get; set; }  // Сколько минут должно выполняться условие
    
    // Действие
    public int? ActuatorId { get; set; }
    public Actuator? Actuator { get; set; }
    public string Action { get; set; } = string.Empty;  // "turn_on", "turn_off"
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastTriggeredAt { get; set; }
    
    // Навигационные свойства
    public ICollection<RuleExecutionLog> ExecutionLogs { get; set; } = new List<RuleExecutionLog>();
}

public class RuleExecutionLog
{
    public long Id { get; set; }
    public int RuleId { get; set; }
    public AutomationRule? Rule { get; set; }
    public bool WasExecuted { get; set; }
    public string? ResultMessage { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}

public class Alert
{
    public long Id { get; set; }
    public int? SensorId { get; set; }
    public Sensor? Sensor { get; set; }
    public int? ActuatorId { get; set; }
    public Actuator? Actuator { get; set; }
    public AlertSeverity Severity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsAcknowledged { get; set; } = false;
    public DateTime? AcknowledgedAt { get; set; }
    public string? AcknowledgedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class AuditLog
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ApplicationUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Operator;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    
    // Навигационные свойства
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

public class RefreshToken
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class NotificationChannel
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    public string ChannelType { get; set; } = string.Empty;  // "Telegram", "Email"
    public string ChannelAddress { get; set; } = string.Empty;  // Telegram chat ID или email
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class NotificationLog
{
    public long Id { get; set; }
    public int? AlertId { get; set; }
    public Alert? Alert { get; set; }
    public int ChannelId { get; set; }
    public NotificationChannel? Channel { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsSent { get; set; } = false;
    public string? ErrorMessage { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
