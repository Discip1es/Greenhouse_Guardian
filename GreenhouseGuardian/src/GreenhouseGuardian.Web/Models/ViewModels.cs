namespace GreenhouseGuardian.Web.Models;

public class SensorViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? ZoneId { get; set; }
    public decimal CurrentValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime LastUpdate { get; set; }
}

public class ActuatorViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? ZoneId { get; set; }
    public string State { get; set; } = string.Empty;
    public bool IsAutoMode { get; set; }
    public DateTime? LastCommandAt { get; set; }
}

public class ZoneViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PlantProfileName { get; set; }
    public int SensorsCount { get; set; }
    public int ActuatorsCount { get; set; }
    public Dictionary<string, decimal> CurrentReadings { get; set; } = new();
}

public class AlertViewModel
{
    public long Id { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsAcknowledged { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? SensorName { get; set; }
}

public class RuleViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool IsPaused { get; set; }
    public string ConditionOperator { get; set; } = string.Empty;
    public decimal ThresholdValue { get; set; }
    public int DurationMinutes { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? SensorName { get; set; }
    public string? ActuatorName { get; set; }
}

public class DashboardStats
{
    public int TotalSensors { get; set; }
    public int ActiveAlerts { get; set; }
    public int CriticalAlerts { get; set; }
    public int ZonesCount { get; set; }
    public int ActuatorsOn { get; set; }
    public double AverageTemperature { get; set; }
    public double AverageHumidity { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserInfo User { get; set; } = new();
}

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
