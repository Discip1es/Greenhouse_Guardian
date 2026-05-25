using GreenhouseGuardian.Domain.Entities;
using GreenhouseGuardian.Infrastructure.Data;
using GreenhouseGuardian.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace GreenhouseGuardian.Tests.Infrastructure;

public class SensorServiceTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly Mock<ICurrentStateCache> _mockCache;
    private readonly Mock<ILogger<SensorService>> _mockLogger;
    private readonly SensorService _sensorService;

    public SensorServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _mockContext = new Mock<ApplicationDbContext>(options);
        _mockCache = new Mock<ICurrentStateCache>();
        _mockLogger = new Mock<ILogger<SensorService>>();
        _sensorService = new SensorService(_mockContext.Object, _mockCache.Object, _mockLogger.Object);
    }

    [Fact]
    public void SensorService_Constructor_ShouldInitialize()
    {
        // Arrange & Act & Assert
        Assert.NotNull(_sensorService);
    }

    [Fact]
    public async Task GetAllSensorsAsync_ShouldReturnAllSensors()
    {
        // Arrange
        var sensors = new List<Sensor>
        {
            new Sensor { Id = 1, Name = "Sensor 1", Code = "SENS-001" },
            new Sensor { Id = 2, Name = "Sensor 2", Code = "SENS-002" }
        }.AsQueryable();

        var mockSet = new Mock<DbSet<Sensor>>();
        mockSet.As<IQueryable<Sensor>>().Setup(m => m.Provider).Returns(sensors.Provider);
        mockSet.As<IQueryable<Sensor>>().Setup(m => m.Expression).Returns(sensors.Expression);
        mockSet.As<IQueryable<Sensor>>().Setup(m => m.ElementType).Returns(sensors.ElementType);
        mockSet.As<IQueryable<Sensor>>().Setup(m => m.GetEnumerator()).Returns(sensors.GetEnumerator());

        _mockContext.Setup(c => c.Sensors).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sensorService.GetAllSensorsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetSensorByIdAsync_ExistingSensor_ShouldReturnSensor()
    {
        // Arrange
        var sensorId = 1;
        var sensor = new Sensor { Id = sensorId, Name = "Test Sensor", Code = "TEST-001" };
        var sensors = new List<Sensor> { sensor }.AsQueryable();

        var mockSet = new Mock<DbSet<Sensor>>();
        mockSet.As<IQueryable<Sensor>>().Setup(m => m.Provider).Returns(sensors.Provider);
        mockSet.As<IQueryable<Sensor>>().Setup(m => m.Expression).Returns(sensors.Expression);
        mockSet.As<IQueryable<Sensor>>().Setup(m => m.ElementType).Returns(sensors.ElementType);
        mockSet.As<IQueryable<Sensor>>().Setup(m => m.GetEnumerator()).Returns(sensors.GetEnumerator());
        mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sensor);

        _mockContext.Setup(c => c.Sensors).Returns(mockSet.Object);

        // Act
        var result = await _sensorService.GetSensorByIdAsync(sensorId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(sensorId, result.Id);
        Assert.Equal("Test Sensor", result.Name);
    }

    [Fact]
    public async Task CreateSensorAsync_ShouldAddSensor()
    {
        // Arrange
        var sensor = new Sensor { Name = "New Sensor", Code = "NEW-001", Type = SensorType.TemperatureAir };

        _mockContext.Setup(c => c.Sensors.AddAsync(sensor, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Sensor>(sensor));
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sensorService.CreateSensorAsync(sensor);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("NEW-001", result.Code);
        _mockContext.Verify(c => c.Sensors.AddAsync(sensor, It.IsAny<CancellationToken>()), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateSensorAsync_ShouldTrimAndUppercaseCode()
    {
        // Arrange
        var sensor = new Sensor { Name = "New Sensor", Code = "  new-001  ", Type = SensorType.TemperatureAir };

        _mockContext.Setup(c => c.Sensors.AddAsync(sensor, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Sensor>(sensor));
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sensorService.CreateSensorAsync(sensor);

        // Assert
        Assert.Equal("NEW-001", result.Code);
    }

    [Fact]
    public async Task UpdateSensorAsync_ExistingSensor_ShouldUpdateSensor()
    {
        // Arrange
        var existingSensor = new Sensor { Id = 1, Name = "Old Name", Code = "OLD-001" };
        var updatedSensor = new Sensor { Id = 1, Name = "New Name", Code = "NEW-001", IsActive = false };

        var sensors = new List<Sensor> { existingSensor }.AsQueryable();
        var mockSet = new Mock<DbSet<Sensor>>();
        mockSet.As<IQueryable<Sensor>>().Setup(m => m.Provider).Returns(sensors.Provider);
        mockSet.As<IQueryable<Sensor>>().Setup(m => m.Expression).Returns(sensors.Expression);
        mockSet.As<IQueryable<Sensor>>().Setup(m => m.ElementType).Returns(sensors.ElementType);
        mockSet.As<IQueryable<Sensor>>().Setup(m => m.GetEnumerator()).Returns(sensors.GetEnumerator());
        mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSensor);

        _mockContext.Setup(c => c.Sensors).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sensorService.UpdateSensorAsync(updatedSensor);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
        Assert.False(result.IsActive);
    }

    [Fact]
    public async Task UpdateSensorAsync_NonExistingSensor_ShouldThrowException()
    {
        // Arrange
        var sensor = new Sensor { Id = 999, Name = "Non-existing" };

        var mockSet = new Mock<DbSet<Sensor>>();
        mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sensor?)null);

        _mockContext.Setup(c => c.Sensors).Returns(mockSet.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sensorService.UpdateSensorAsync(sensor));
    }

    [Fact]
    public async Task DeleteSensorAsync_ExistingSensor_ShouldDeleteSensor()
    {
        // Arrange
        var sensor = new Sensor { Id = 1, Name = "To Delete", Code = "DEL-001" };

        var mockSet = new Mock<DbSet<Sensor>>();
        mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sensor);

        _mockContext.Setup(c => c.Sensors).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sensorService.DeleteSensorAsync(1);

        // Assert
        _mockContext.Verify(c => c.Sensors.Remove(sensor), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCurrentReadingsAsync_ShouldReturnReadingsFromCache()
    {
        // Arrange
        var cacheValues = new Dictionary<int, decimal>
        {
            { 1, 25.5m },
            { 2, 30.0m }
        };

        _mockCache.Setup(c => c.GetAllSensorCurrentValuesAsync())
            .ReturnsAsync(cacheValues);

        // Act
        var result = await _sensorService.GetCurrentReadingsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }
}

public class ActuatorServiceTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly Mock<ICurrentStateCache> _mockCache;
    private readonly ActuatorService _actuatorService;

    public ActuatorServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _mockContext = new Mock<ApplicationDbContext>(options);
        _mockCache = new Mock<ICurrentStateCache>();
        _actuatorService = new ActuatorService(_mockContext.Object, _mockCache.Object);
    }

    [Fact]
    public void ActuatorService_Constructor_ShouldInitialize()
    {
        // Arrange & Act & Assert
        Assert.NotNull(_actuatorService);
    }

    [Fact]
    public async Task ExecuteCommandAsync_TurnOn_ShouldSetActuatorStateToOn()
    {
        // Arrange
        var actuator = new Actuator { Id = 1, Name = "Fan 1", Code = "FAN-001", State = ActuatorState.Off };

        var mockSet = new Mock<DbSet<Actuator>>();
        mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(actuator);

        _mockContext.Setup(c => c.Actuators).Returns(mockSet.Object);
        _mockContext.Setup(c => c.ActuatorLogs.AddAsync(It.IsAny<ActuatorLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ActuatorLog>(new ActuatorLog()));
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockCache.Setup(c => c.SetActuatorStateAsync(1, ActuatorState.On))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _actuatorService.ExecuteCommandAsync(1, "on", "user-123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ActuatorState.On, result.State);
        Assert.Equal("user-123", result.LastCommandBy);
    }

    [Fact]
    public async Task ExecuteCommandAsync_TurnOff_ShouldSetActuatorStateToOff()
    {
        // Arrange
        var actuator = new Actuator { Id = 1, Name = "Fan 1", Code = "FAN-001", State = ActuatorState.On };

        var mockSet = new Mock<DbSet<Actuator>>();
        mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(actuator);

        _mockContext.Setup(c => c.Actuators).Returns(mockSet.Object);
        _mockContext.Setup(c => c.ActuatorLogs.AddAsync(It.IsAny<ActuatorLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ActuatorLog>(new ActuatorLog()));
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockCache.Setup(c => c.SetActuatorStateAsync(1, ActuatorState.Off))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _actuatorService.ExecuteCommandAsync(1, "off", "user-123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ActuatorState.Off, result.State);
    }

    [Fact]
    public async Task ExecuteCommandAsync_InvalidAction_ShouldThrowException()
    {
        // Arrange
        var actuator = new Actuator { Id = 1, Name = "Fan 1", Code = "FAN-001" };

        var mockSet = new Mock<DbSet<Actuator>>();
        mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(actuator);

        _mockContext.Setup(c => c.Actuators).Returns(mockSet.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _actuatorService.ExecuteCommandAsync(1, "invalid"));
    }

    [Fact]
    public async Task ExecuteCommandAsync_NonExistingActuator_ShouldThrowException()
    {
        // Arrange
        var mockSet = new Mock<DbSet<Actuator>>();
        mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Actuator?)null);

        _mockContext.Setup(c => c.Actuators).Returns(mockSet.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _actuatorService.ExecuteCommandAsync(999, "on"));
    }

    [Fact]
    public async Task GetAllActuatorsAsync_ShouldReturnAllActuators()
    {
        // Arrange
        var actuators = new List<Actuator>
        {
            new Actuator { Id = 1, Name = "Fan 1", Code = "FAN-001" },
            new Actuator { Id = 2, Name = "Valve 1", Code = "VALVE-001" }
        }.AsQueryable();

        var mockSet = new Mock<DbSet<Actuator>>();
        mockSet.As<IQueryable<Actuator>>().Setup(m => m.Provider).Returns(actuators.Provider);
        mockSet.As<IQueryable<Actuator>>().Setup(m => m.Expression).Returns(actuators.Expression);
        mockSet.As<IQueryable<Actuator>>().Setup(m => m.ElementType).Returns(actuators.ElementType);
        mockSet.As<IQueryable<Actuator>>().Setup(m => m.GetEnumerator()).Returns(actuators.GetEnumerator());

        _mockContext.Setup(c => c.Actuators).Returns(mockSet.Object);

        // Act
        var result = await _actuatorService.GetAllActuatorsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }
}

public class CurrentStateCacheTests
{
    private readonly CurrentStateCache _cache;

    public CurrentStateCacheTests()
    {
        _cache = new CurrentStateCache();
    }

    [Fact]
    public async Task SetSensorCurrentValueAsync_ShouldStoreValue()
    {
        // Arrange
        var sensorId = 1;
        var value = 25.5m;

        // Act
        await _cache.SetSensorCurrentValueAsync(sensorId, value);

        // Assert
        var result = await _cache.GetSensorCurrentValueAsync(sensorId);
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task GetSensorCurrentValueAsync_NonExistingKey_ShouldReturnNull()
    {
        // Act
        var result = await _cache.GetSensorCurrentValueAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllSensorCurrentValuesAsync_ShouldReturnAllValues()
    {
        // Arrange
        await _cache.SetSensorCurrentValueAsync(1, 25.5m);
        await _cache.SetSensorCurrentValueAsync(2, 30.0m);
        await _cache.SetSensorCurrentValueAsync(3, 22.0m);

        // Act
        var result = await _cache.GetAllSensorCurrentValuesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal(25.5m, result[1]);
        Assert.Equal(30.0m, result[2]);
        Assert.Equal(22.0m, result[3]);
    }

    [Fact]
    public async Task SetActuatorStateAsync_ShouldStoreState()
    {
        // Arrange
        var actuatorId = 1;
        var state = ActuatorState.On;

        // Act
        await _cache.SetActuatorStateAsync(actuatorId, state);

        // Assert
        var result = await _cache.GetActuatorStateAsync(actuatorId);
        Assert.Equal(state, result);
    }

    [Fact]
    public async Task GetActuatorStateAsync_NonExistingKey_ShouldReturnNull()
    {
        // Act
        var result = await _cache.GetActuatorStateAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllActuatorStatesAsync_ShouldReturnAllStates()
    {
        // Arrange
        await _cache.SetActuatorStateAsync(1, ActuatorState.On);
        await _cache.SetActuatorStateAsync(2, ActuatorState.Off);
        await _cache.SetActuatorStateAsync(3, ActuatorState.InProgress);

        // Act
        var result = await _cache.GetAllActuatorStatesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal(ActuatorState.On, result[1]);
        Assert.Equal(ActuatorState.Off, result[2]);
        Assert.Equal(ActuatorState.InProgress, result[3]);
    }
}
