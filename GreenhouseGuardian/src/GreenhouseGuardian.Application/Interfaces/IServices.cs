using GreenhouseGuardian.Domain.Entities;

namespace GreenhouseGuardian.Application.Interfaces;

public interface ISensorService
{
    Task<IEnumerable<Sensor>> GetAllSensorsAsync();
    Task<Sensor?> GetSensorByIdAsync(int id);
    Task<Sensor> CreateSensorAsync(Sensor sensor);
    Task<Sensor> UpdateSensorAsync(Sensor sensor);
    Task DeleteSensorAsync(int id);
    Task<IEnumerable<SensorReading>> GetSensorHistoryAsync(int sensorId, DateTime from, DateTime to, string? interval = null);
    Task<IEnumerable<SensorReading>> GetCurrentReadingsAsync();
}

public interface IActuatorService
{
    Task<IEnumerable<Actuator>> GetAllActuatorsAsync();
    Task<Actuator?> GetActuatorByIdAsync(int id);
    Task<Actuator> ExecuteCommandAsync(int actuatorId, string action, string? userId = null);
    Task<IEnumerable<ActuatorLog>> GetActuatorLogsAsync(int actuatorId, DateTime? from = null, DateTime? to = null);
}

public interface IZoneService
{
    Task<IEnumerable<Zone>> GetAllZonesAsync();
    Task<Zone?> GetZoneByIdAsync(string id);
    Task<Zone> CreateZoneAsync(Zone zone);
    Task<Zone> UpdateZoneAsync(Zone zone);
    Task DeleteZoneAsync(string id);
}

public interface IPlantProfileService
{
    Task<IEnumerable<PlantProfile>> GetAllProfilesAsync();
    Task<PlantProfile?> GetProfileByIdAsync(int id);
    Task<PlantProfile> CreateProfileAsync(PlantProfile profile);
    Task<PlantProfile> UpdateProfileAsync(PlantProfile profile);
    Task DeleteProfileAsync(int id);
    Task<IEnumerable<string>> GetRecommendationsAsync(string zoneId);
}

public interface IAutomationRuleService
{
    Task<IEnumerable<AutomationRule>> GetAllRulesAsync();
    Task<AutomationRule?> GetRuleByIdAsync(int id);
    Task<AutomationRule> CreateRuleAsync(AutomationRule rule);
    Task<AutomationRule> UpdateRuleAsync(AutomationRule rule);
    Task DeleteRuleAsync(int id);
    Task PauseRuleAsync(int id);
    Task ResumeRuleAsync(int id);
    Task EvaluateRulesAsync();
}

public interface IAlertService
{
    Task<IEnumerable<Alert>> GetAllAlertsAsync(bool? isAcknowledged = null);
    Task<Alert> AcknowledgeAlertAsync(long alertId, string userId);
    Task CreateAlertAsync(Alert alert);
}

public interface IAuthService
{
    Task<(string AccessToken, string RefreshToken)> LoginAsync(string email, string password);
    Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string userId);
    Task<ApplicationUser?> GetUserByIdAsync(string id);
    Task<ApplicationUser?> GetUserByEmailAsync(string email);
}

public interface IAuditService
{
    Task LogActionAsync(string userId, string action, string entityType, int? entityId, string? oldValues, string? newValues, string ipAddress);
    Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string? userId = null, string? action = null, DateTime? from = null, DateTime? to = null);
}

public interface ITelemetryCollector
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
    Task<IEnumerable<SensorReading>> CollectLatestReadingsAsync();
}

public interface INotificationService
{
    Task SendNotificationAsync(int alertId, string channelType, string channelAddress);
    Task<IEnumerable<NotificationLog>> GetNotificationHistoryAsync(int? alertId = null);
}

public interface ICurrentStateCache
{
    Task<IDictionary<int, decimal>> GetAllSensorCurrentValuesAsync();
    Task<decimal?> GetSensorCurrentValueAsync(int sensorId);
    Task SetSensorCurrentValueAsync(int sensorId, decimal value);
    Task<Dictionary<int, ActuatorState>> GetAllActuatorStatesAsync();
    Task<ActuatorState?> GetActuatorStateAsync(int actuatorId);
    Task SetActuatorStateAsync(int actuatorId, ActuatorState state);
}
