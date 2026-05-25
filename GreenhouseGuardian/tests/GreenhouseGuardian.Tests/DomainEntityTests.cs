using GreenhouseGuardian.Domain.Entities;

namespace GreenhouseGuardian.Tests.Domain;

public class SensorTests
{
    [Fact]
    public void Sensor_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var sensor = new Sensor();

        // Assert
        Assert.True(sensor.IsActive);
        Assert.Equal(0, sensor.Id);
        Assert.Empty(sensor.Name);
        Assert.Empty(sensor.Code);
        Assert.Equal(SensorType.TemperatureAir, sensor.Type);
        Assert.Null(sensor.ZoneId);
        Assert.Equal(0m, sensor.WarningThresholdLow);
        Assert.Equal(0m, sensor.WarningThresholdHigh);
        Assert.Equal(0m, sensor.CriticalThresholdLow);
        Assert.Equal(0m, sensor.CriticalThresholdHigh);
        Assert.Empty(sensor.Unit);
        Assert.NotNull(sensor.Readings);
        Assert.NotNull(sensor.Alerts);
    }

    [Fact]
    public void Sensor_Initialization_ShouldSetProperties()
    {
        // Arrange
        var sensor = new Sensor
        {
            Id = 1,
            Name = "Temperature Sensor 1",
            Code = "TEMP-001",
            Type = SensorType.TemperatureAir,
            ZoneId = "zone-1",
            WarningThresholdLow = 15m,
            WarningThresholdHigh = 30m,
            CriticalThresholdLow = 10m,
            CriticalThresholdHigh = 40m,
            Unit = "°C",
            IsActive = true
        };

        // Assert
        Assert.Equal(1, sensor.Id);
        Assert.Equal("Temperature Sensor 1", sensor.Name);
        Assert.Equal("TEMP-001", sensor.Code);
        Assert.Equal(SensorType.TemperatureAir, sensor.Type);
        Assert.Equal("zone-1", sensor.ZoneId);
        Assert.Equal(15m, sensor.WarningThresholdLow);
        Assert.Equal(30m, sensor.WarningThresholdHigh);
        Assert.Equal(10m, sensor.CriticalThresholdLow);
        Assert.Equal(40m, sensor.CriticalThresholdHigh);
        Assert.Equal("°C", sensor.Unit);
        Assert.True(sensor.IsActive);
    }

    [Fact]
    public void Sensor_ReadingsCollection_ShouldBeEmptyInitially()
    {
        // Arrange
        var sensor = new Sensor();

        // Assert
        Assert.Empty(sensor.Readings);
    }

    [Fact]
    public void Sensor_AlertsCollection_ShouldBeEmptyInitially()
    {
        // Arrange
        var sensor = new Sensor();

        // Assert
        Assert.Empty(sensor.Alerts);
    }

    [Fact]
    public void Sensor_CreatedAt_ShouldBeUtcNow()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var sensor = new Sensor();

        // Assert
        Assert.InRange(sensor.CreatedAt, beforeCreation.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }
}

public class SensorReadingTests
{
    [Fact]
    public void SensorReading_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var reading = new SensorReading();

        // Assert
        Assert.Equal(0, reading.Id);
        Assert.Equal(0, reading.SensorId);
        Assert.Equal(0m, reading.Value);
        Assert.Null(reading.Quality);
        Assert.InRange(reading.Timestamp, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void SensorReading_Initialization_ShouldSetProperties()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var reading = new SensorReading
        {
            Id = 1,
            SensorId = 5,
            Value = 25.5m,
            Quality = "Good",
            Timestamp = timestamp
        };

        // Assert
        Assert.Equal(1, reading.Id);
        Assert.Equal(5, reading.SensorId);
        Assert.Equal(25.5m, reading.Value);
        Assert.Equal("Good", reading.Quality);
        Assert.Equal(timestamp, reading.Timestamp);
    }
}

public class ActuatorTests
{
    [Fact]
    public void Actuator_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var actuator = new Actuator();

        // Assert
        Assert.True(actuator.IsActive);
        Assert.Equal(ActuatorState.Off, actuator.State);
        Assert.False(actuator.IsAutoMode);
        Assert.Equal(0, actuator.Id);
        Assert.Empty(actuator.Name);
        Assert.Empty(actuator.Code);
        Assert.Equal(ActuatorType.IrrigationValve, actuator.Type);
        Assert.Null(actuator.ZoneId);
        Assert.Null(actuator.LastCommandAt);
        Assert.Null(actuator.LastCommandBy);
        Assert.NotNull(actuator.CommandLogs);
    }

    [Fact]
    public void Actuator_Initialization_ShouldSetProperties()
    {
        // Arrange
        var lastCommandAt = DateTime.UtcNow;
        var actuator = new Actuator
        {
            Id = 1,
            Name = "Ventilation Fan 1",
            Code = "FAN-001",
            Type = ActuatorType.VentilationFan,
            ZoneId = "zone-1",
            State = ActuatorState.On,
            IsAutoMode = true,
            LastCommandAt = lastCommandAt,
            LastCommandBy = "user-123"
        };

        // Assert
        Assert.Equal(1, actuator.Id);
        Assert.Equal("Ventilation Fan 1", actuator.Name);
        Assert.Equal("FAN-001", actuator.Code);
        Assert.Equal(ActuatorType.VentilationFan, actuator.Type);
        Assert.Equal("zone-1", actuator.ZoneId);
        Assert.Equal(ActuatorState.On, actuator.State);
        Assert.True(actuator.IsAutoMode);
        Assert.Equal(lastCommandAt, actuator.LastCommandAt);
        Assert.Equal("user-123", actuator.LastCommandBy);
    }

    [Fact]
    public void Actuator_CommandLogsCollection_ShouldBeEmptyInitially()
    {
        // Arrange
        var actuator = new Actuator();

        // Assert
        Assert.Empty(actuator.CommandLogs);
    }
}

public class ActuatorLogTests
{
    [Fact]
    public void ActuatorLog_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var log = new ActuatorLog();

        // Assert
        Assert.Equal(0, log.Id);
        Assert.Equal(0, log.ActuatorId);
        Assert.Equal(ActuatorState.Off, log.PreviousState);
        Assert.Equal(ActuatorState.Off, log.NewState);
        Assert.Null(log.TriggeredBy);
        Assert.Null(log.Reason);
        Assert.InRange(log.ExecutedAt, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void ActuatorLog_Initialization_ShouldSetProperties()
    {
        // Arrange
        var executedAt = DateTime.UtcNow;
        var log = new ActuatorLog
        {
            Id = 1,
            ActuatorId = 5,
            PreviousState = ActuatorState.Off,
            NewState = ActuatorState.On,
            TriggeredBy = "rule-123",
            Reason = "Temperature threshold exceeded",
            ExecutedAt = executedAt
        };

        // Assert
        Assert.Equal(1, log.Id);
        Assert.Equal(5, log.ActuatorId);
        Assert.Equal(ActuatorState.Off, log.PreviousState);
        Assert.Equal(ActuatorState.On, log.NewState);
        Assert.Equal("rule-123", log.TriggeredBy);
        Assert.Equal("Temperature threshold exceeded", log.Reason);
        Assert.Equal(executedAt, log.ExecutedAt);
    }
}

public class ZoneTests
{
    [Fact]
    public void Zone_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var zone = new Zone();

        // Assert
        Assert.NotEmpty(zone.Id);
        Assert.Empty(zone.Name);
        Assert.Null(zone.Description);
        Assert.Null(zone.PlantProfileId);
        Assert.NotNull(zone.Sensors);
        Assert.NotNull(zone.Actuators);
        Assert.InRange(zone.CreatedAt, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void Zone_Initialization_ShouldSetProperties()
    {
        // Arrange
        var zone = new Zone
        {
            Id = "custom-zone-id",
            Name = "Greenhouse A",
            Description = "Main greenhouse for tomatoes",
            PlantProfileId = "profile-1"
        };

        // Assert
        Assert.Equal("custom-zone-id", zone.Id);
        Assert.Equal("Greenhouse A", zone.Name);
        Assert.Equal("Main greenhouse for tomatoes", zone.Description);
        Assert.Equal("profile-1", zone.PlantProfileId);
    }

    [Fact]
    public void Zone_Id_ShouldBeGuid()
    {
        // Arrange & Act
        var zone = new Zone();

        // Assert - Verify it's a valid GUID string
        Assert.True(Guid.TryParse(zone.Id, out _));
    }

    [Fact]
    public void Zone_SensorsCollection_ShouldBeEmptyInitially()
    {
        // Arrange
        var zone = new Zone();

        // Assert
        Assert.Empty(zone.Sensors);
    }

    [Fact]
    public void Zone_ActuatorsCollection_ShouldBeEmptyInitially()
    {
        // Arrange
        var zone = new Zone();

        // Assert
        Assert.Empty(zone.Actuators);
    }
}

public class PlantProfileTests
{
    [Fact]
    public void PlantProfile_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var profile = new PlantProfile();

        // Assert
        Assert.Equal(0, profile.Id);
        Assert.Empty(profile.Name);
        Assert.Empty(profile.CultureName);
        Assert.Null(profile.Description);
        Assert.Equal(0m, profile.TempDayMin);
        Assert.Equal(0m, profile.TempDayMax);
        Assert.Equal(0m, profile.TempNightMin);
        Assert.Equal(0m, profile.TempNightMax);
        Assert.Equal(0m, profile.HumidityMin);
        Assert.Equal(0m, profile.HumidityMax);
        Assert.Equal(0m, profile.LightMin);
        Assert.Equal(0m, profile.LightMax);
        Assert.Equal(0m, profile.CO2Min);
        Assert.Equal(0m, profile.CO2Max);
        Assert.Equal(0m, profile.PHMin);
        Assert.Equal(0m, profile.PHMax);
        Assert.Equal(0m, profile.ECMin);
        Assert.Equal(0m, profile.ECMax);
        Assert.NotNull(profile.Zones);
        Assert.InRange(profile.CreatedAt, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void PlantProfile_Initialization_ShouldSetProperties()
    {
        // Arrange
        var profile = new PlantProfile
        {
            Id = 1,
            Name = "Tomato Profile",
            CultureName = "Tomatoes",
            Description = "Optimal conditions for tomato growth",
            TempDayMin = 20m,
            TempDayMax = 28m,
            TempNightMin = 15m,
            TempNightMax = 20m,
            HumidityMin = 60m,
            HumidityMax = 80m,
            LightMin = 10000m,
            LightMax = 50000m,
            CO2Min = 400m,
            CO2Max = 1000m,
            PHMin = 6.0m,
            PHMax = 7.0m,
            ECMin = 2.0m,
            ECMax = 3.5m
        };

        // Assert
        Assert.Equal(1, profile.Id);
        Assert.Equal("Tomato Profile", profile.Name);
        Assert.Equal("Tomatoes", profile.CultureName);
        Assert.Equal("Optimal conditions for tomato growth", profile.Description);
        Assert.Equal(20m, profile.TempDayMin);
        Assert.Equal(28m, profile.TempDayMax);
        Assert.Equal(15m, profile.TempNightMin);
        Assert.Equal(20m, profile.TempNightMax);
        Assert.Equal(60m, profile.HumidityMin);
        Assert.Equal(80m, profile.HumidityMax);
        Assert.Equal(10000m, profile.LightMin);
        Assert.Equal(50000m, profile.LightMax);
        Assert.Equal(400m, profile.CO2Min);
        Assert.Equal(1000m, profile.CO2Max);
        Assert.Equal(6.0m, profile.PHMin);
        Assert.Equal(7.0m, profile.PHMax);
        Assert.Equal(2.0m, profile.ECMin);
        Assert.Equal(3.5m, profile.ECMax);
    }

    [Fact]
    public void PlantProfile_ZonesCollection_ShouldBeEmptyInitially()
    {
        // Arrange
        var profile = new PlantProfile();

        // Assert
        Assert.Empty(profile.Zones);
    }
}

public class AutomationRuleTests
{
    [Fact]
    public void AutomationRule_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var rule = new AutomationRule();

        // Assert
        Assert.True(rule.IsActive);
        Assert.False(rule.IsPaused);
        Assert.Equal(0, rule.Id);
        Assert.Empty(rule.Name);
        Assert.Null(rule.Description);
        Assert.Null(rule.SensorId);
        Assert.Empty(rule.ConditionOperator);
        Assert.Equal(0m, rule.ThresholdValue);
        Assert.Equal(0, rule.DurationMinutes);
        Assert.Null(rule.ActuatorId);
        Assert.Empty(rule.Action);
        Assert.Null(rule.LastTriggeredAt);
        Assert.NotNull(rule.ExecutionLogs);
        Assert.InRange(rule.CreatedAt, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void AutomationRule_Initialization_ShouldSetProperties()
    {
        // Arrange
        var rule = new AutomationRule
        {
            Id = 1,
            Name = "High Temperature Rule",
            Description = "Turn on ventilation when temperature is too high",
            IsActive = true,
            IsPaused = false,
            SensorId = 5,
            ConditionOperator = "gt",
            ThresholdValue = 30m,
            DurationMinutes = 5,
            ActuatorId = 3,
            Action = "turn_on"
        };

        // Assert
        Assert.Equal(1, rule.Id);
        Assert.Equal("High Temperature Rule", rule.Name);
        Assert.Equal("Turn on ventilation when temperature is too high", rule.Description);
        Assert.True(rule.IsActive);
        Assert.False(rule.IsPaused);
        Assert.Equal(5, rule.SensorId);
        Assert.Equal("gt", rule.ConditionOperator);
        Assert.Equal(30m, rule.ThresholdValue);
        Assert.Equal(5, rule.DurationMinutes);
        Assert.Equal(3, rule.ActuatorId);
        Assert.Equal("turn_on", rule.Action);
    }

    [Fact]
    public void AutomationRule_ExecutionLogsCollection_ShouldBeEmptyInitially()
    {
        // Arrange
        var rule = new AutomationRule();

        // Assert
        Assert.Empty(rule.ExecutionLogs);
    }
}

public class AlertTests
{
    [Fact]
    public void Alert_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var alert = new Alert();

        // Assert
        Assert.Equal(0, alert.Id);
        Assert.Null(alert.SensorId);
        Assert.Null(alert.ActuatorId);
        Assert.Equal(AlertSeverity.Info, alert.Severity);
        Assert.Empty(alert.Title);
        Assert.Empty(alert.Message);
        Assert.False(alert.IsAcknowledged);
        Assert.Null(alert.AcknowledgedAt);
        Assert.Null(alert.AcknowledgedBy);
        Assert.InRange(alert.CreatedAt, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void Alert_Initialization_ShouldSetProperties()
    {
        // Arrange
        var createdAt = DateTime.UtcNow;
        var alert = new Alert
        {
            Id = 1,
            SensorId = 5,
            ActuatorId = 3,
            Severity = AlertSeverity.Critical,
            Title = "Critical Temperature Alert",
            Message = "Temperature exceeded critical threshold",
            IsAcknowledged = true,
            AcknowledgedAt = createdAt,
            AcknowledgedBy = "user-123",
            CreatedAt = createdAt
        };

        // Assert
        Assert.Equal(1, alert.Id);
        Assert.Equal(5, alert.SensorId);
        Assert.Equal(3, alert.ActuatorId);
        Assert.Equal(AlertSeverity.Critical, alert.Severity);
        Assert.Equal("Critical Temperature Alert", alert.Title);
        Assert.Equal("Temperature exceeded critical threshold", alert.Message);
        Assert.True(alert.IsAcknowledged);
        Assert.Equal(createdAt, alert.AcknowledgedAt);
        Assert.Equal("user-123", alert.AcknowledgedBy);
    }
}

public class ApplicationUserTests
{
    [Fact]
    public void ApplicationUser_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var user = new ApplicationUser();

        // Assert
        Assert.NotEmpty(user.Id);
        Assert.Empty(user.Email);
        Assert.Empty(user.UserName);
        Assert.Empty(user.PasswordHash);
        Assert.Equal(UserRole.Operator, user.Role);
        Assert.True(user.IsActive);
        Assert.NotNull(user.RefreshTokens);
        Assert.InRange(user.CreatedAt, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        Assert.Null(user.LastLoginAt);
    }

    [Fact]
    public void ApplicationUser_Id_ShouldBeGuid()
    {
        // Arrange & Act
        var user = new ApplicationUser();

        // Assert - Verify it's a valid GUID string
        Assert.True(Guid.TryParse(user.Id, out _));
    }

    [Fact]
    public void ApplicationUser_Initialization_ShouldSetProperties()
    {
        // Arrange
        var createdAt = DateTime.UtcNow;
        var lastLoginAt = DateTime.UtcNow.AddHours(-1);
        var user = new ApplicationUser
        {
            Id = "custom-user-id",
            Email = "test@example.com",
            UserName = "testuser",
            PasswordHash = "hashed-password",
            Role = UserRole.Administrator,
            IsActive = true,
            CreatedAt = createdAt,
            LastLoginAt = lastLoginAt
        };

        // Assert
        Assert.Equal("custom-user-id", user.Id);
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("testuser", user.UserName);
        Assert.Equal("hashed-password", user.PasswordHash);
        Assert.Equal(UserRole.Administrator, user.Role);
        Assert.True(user.IsActive);
        Assert.Equal(createdAt, user.CreatedAt);
        Assert.Equal(lastLoginAt, user.LastLoginAt);
    }

    [Fact]
    public void ApplicationUser_RefreshTokensCollection_ShouldBeEmptyInitially()
    {
        // Arrange
        var user = new ApplicationUser();

        // Assert
        Assert.Empty(user.RefreshTokens);
    }
}

public class RefreshTokenTests
{
    [Fact]
    public void RefreshToken_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var token = new RefreshToken();

        // Assert
        Assert.Equal(0, token.Id);
        Assert.Empty(token.UserId);
        Assert.Empty(token.Token);
        Assert.False(token.IsRevoked);
        Assert.InRange(token.ExpiresAt, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        Assert.InRange(token.CreatedAt, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void RefreshToken_Initialization_ShouldSetProperties()
    {
        // Arrange
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var createdAt = DateTime.UtcNow;
        var token = new RefreshToken
        {
            Id = 1,
            UserId = "user-123",
            Token = "refresh-token-string",
            IsRevoked = true,
            ExpiresAt = expiresAt,
            CreatedAt = createdAt
        };

        // Assert
        Assert.Equal(1, token.Id);
        Assert.Equal("user-123", token.UserId);
        Assert.Equal("refresh-token-string", token.Token);
        Assert.True(token.IsRevoked);
        Assert.Equal(expiresAt, token.ExpiresAt);
        Assert.Equal(createdAt, token.CreatedAt);
    }
}

public class AuditLogTests
{
    [Fact]
    public void AuditLog_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var log = new AuditLog();

        // Assert
        Assert.Equal(0, log.Id);
        Assert.Empty(log.UserId);
        Assert.Empty(log.Action);
        Assert.Empty(log.EntityType);
        Assert.Null(log.EntityId);
        Assert.Null(log.OldValues);
        Assert.Null(log.NewValues);
        Assert.Empty(log.IpAddress);
        Assert.InRange(log.CreatedAt, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void AuditLog_Initialization_ShouldSetProperties()
    {
        // Arrange
        var createdAt = DateTime.UtcNow;
        var log = new AuditLog
        {
            Id = 1,
            UserId = "user-123",
            Action = "Update",
            EntityType = "Sensor",
            EntityId = 5,
            OldValues = "{\"name\": \"old\"}",
            NewValues = "{\"name\": \"new\"}",
            IpAddress = "192.168.1.1",
            CreatedAt = createdAt
        };

        // Assert
        Assert.Equal(1, log.Id);
        Assert.Equal("user-123", log.UserId);
        Assert.Equal("Update", log.Action);
        Assert.Equal("Sensor", log.EntityType);
        Assert.Equal(5, log.EntityId);
        Assert.Equal("{\"name\": \"old\"}", log.OldValues);
        Assert.Equal("{\"name\": \"new\"}", log.NewValues);
        Assert.Equal("192.168.1.1", log.IpAddress);
        Assert.Equal(createdAt, log.CreatedAt);
    }
}

public class NotificationChannelTests
{
    [Fact]
    public void NotificationChannel_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var channel = new NotificationChannel();

        // Assert
        Assert.Equal(0, channel.Id);
        Assert.Empty(channel.UserId);
        Assert.Empty(channel.ChannelType);
        Assert.Empty(channel.ChannelAddress);
        Assert.True(channel.IsActive);
        Assert.InRange(channel.CreatedAt, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void NotificationChannel_Initialization_ShouldSetProperties()
    {
        // Arrange
        var channel = new NotificationChannel
        {
            Id = 1,
            UserId = "user-123",
            ChannelType = "Telegram",
            ChannelAddress = "@telegram_channel",
            IsActive = false
        };

        // Assert
        Assert.Equal(1, channel.Id);
        Assert.Equal("user-123", channel.UserId);
        Assert.Equal("Telegram", channel.ChannelType);
        Assert.Equal("@telegram_channel", channel.ChannelAddress);
        Assert.False(channel.IsActive);
    }
}

public class NotificationLogTests
{
    [Fact]
    public void NotificationLog_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var log = new NotificationLog();

        // Assert
        Assert.Equal(0, log.Id);
        Assert.Null(log.AlertId);
        Assert.Equal(0, log.ChannelId);
        Assert.Empty(log.Message);
        Assert.False(log.IsSent);
        Assert.Null(log.ErrorMessage);
        Assert.InRange(log.SentAt, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void NotificationLog_Initialization_ShouldSetProperties()
    {
        // Arrange
        var sentAt = DateTime.UtcNow;
        var log = new NotificationLog
        {
            Id = 1,
            AlertId = 5,
            ChannelId = 3,
            Message = "Alert notification",
            IsSent = true,
            ErrorMessage = null,
            SentAt = sentAt
        };

        // Assert
        Assert.Equal(1, log.Id);
        Assert.Equal(5, log.AlertId);
        Assert.Equal(3, log.ChannelId);
        Assert.Equal("Alert notification", log.Message);
        Assert.True(log.IsSent);
        Assert.Null(log.ErrorMessage);
        Assert.Equal(sentAt, log.SentAt);
    }
}

public class RuleExecutionLogTests
{
    [Fact]
    public void RuleExecutionLog_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var log = new RuleExecutionLog();

        // Assert
        Assert.Equal(0, log.Id);
        Assert.Equal(0, log.RuleId);
        Assert.False(log.WasExecuted);
        Assert.Null(log.ResultMessage);
        Assert.InRange(log.ExecutedAt, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void RuleExecutionLog_Initialization_ShouldSetProperties()
    {
        // Arrange
        var executedAt = DateTime.UtcNow;
        var log = new RuleExecutionLog
        {
            Id = 1,
            RuleId = 5,
            WasExecuted = true,
            ResultMessage = "Rule executed successfully",
            ExecutedAt = executedAt
        };

        // Assert
        Assert.Equal(1, log.Id);
        Assert.Equal(5, log.RuleId);
        Assert.True(log.WasExecuted);
        Assert.Equal("Rule executed successfully", log.ResultMessage);
        Assert.Equal(executedAt, log.ExecutedAt);
    }
}
