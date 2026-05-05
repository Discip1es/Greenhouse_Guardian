using GreenhouseGuardian.Application.Interfaces;
using GreenhouseGuardian.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace GreenhouseGuardian.API.Hubs;

public interface ITelemetryHubClient
{
    Task TelemetryUpdate(TelemetryUpdateDto update);
    Task ActuatorStateChanged(int actuatorId, ActuatorState state);
    Task AlertNotification(AlertDto alert);
}

public class TelemetryHub : Hub<ITelemetryHubClient>
{
    private readonly ISensorService _sensorService;
    private readonly IActuatorService _actuatorService;
    private readonly ICurrentStateCache _cache;
    private readonly ILogger<TelemetryHub> _logger;
    private static readonly HashSet<string> ConnectedClients = new();

    public TelemetryHub(
        ISensorService sensorService,
        IActuatorService actuatorService,
        ICurrentStateCache cache,
        ILogger<TelemetryHub> logger)
    {
        _sensorService = sensorService;
        _actuatorService = actuatorService;
        _cache = cache;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        ConnectedClients.Add(Context.ConnectionId);
        _logger.LogInformation("Client connected to telemetry hub: {ConnectionId}, Total: {Count}", Context.ConnectionId, ConnectedClients.Count);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        ConnectedClients.Remove(Context.ConnectionId);
        _logger.LogInformation("Client disconnected from telemetry hub: {ConnectionId}, Total: {Count}", Context.ConnectionId, ConnectedClients.Count);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SubscribeToSensors(int[] sensorIds)
    {
        if (sensorIds.Length > 0)
        {
            foreach (var sensorId in sensorIds)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"sensor-{sensorId}");
            }
        }
        else
        {
            // Subscribe to all sensors
            await Groups.AddToGroupAsync(Context.ConnectionId, "all-sensors");
        }
        
        _logger.LogInformation("Client {ConnectionId} subscribed to sensors", Context.ConnectionId);
    }

    public async Task UnsubscribeFromSensors(int[] sensorIds)
    {
        foreach (var sensorId in sensorIds)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"sensor-{sensorId}");
        }
        _logger.LogInformation("Client {ConnectionId} unsubscribed from sensors", Context.ConnectionId);
    }
}

public class TelemetryBroadcaster : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<TelemetryHub, ITelemetryHubClient> _hubContext;
    private readonly ILogger<TelemetryBroadcaster> _logger;
    private readonly int _broadcastIntervalSeconds;

    public TelemetryBroadcaster(
        IServiceProvider serviceProvider,
        IHubContext<TelemetryHub, ITelemetryHubClient> hubContext,
        ILogger<TelemetryBroadcaster> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
        _logger = logger;
        _broadcastIntervalSeconds = configuration.GetValue<int>("EmulatorSettings:SignalRBroadcastIntervalSeconds", 2);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Telemetry broadcaster started with interval {Interval}s", _broadcastIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var sensorService = scope.ServiceProvider.GetRequiredService<ISensorService>();
                var actuatorService = scope.ServiceProvider.GetRequiredService<IActuatorService>();
                var cache = scope.ServiceProvider.GetRequiredService<ICurrentStateCache>();

                var now = DateTime.UtcNow;
                
                // Get current sensor values
                var sensorValues = await cache.GetAllSensorCurrentValuesAsync();
                var sensors = await sensorService.GetAllSensorsAsync();
                
                var sensorUpdates = new List<SensorTelemetryDto>();
                foreach (var sensor in sensors)
                {
                    if (sensorValues.TryGetValue(sensor.Id, out var value))
                    {
                        var status = DetermineStatus(value, sensor);
                        sensorUpdates.Add(new SensorTelemetryDto
                        {
                            SensorId = sensor.Id,
                            Code = sensor.Code,
                            Type = sensor.Type,
                            Value = value,
                            Unit = sensor.Unit,
                            Status = status
                        });
                    }
                }

                // Get actuator states
                var actuatorStates = await cache.GetAllActuatorStatesAsync();
                var actuators = await actuatorService.GetAllActuatorsAsync();
                
                var actuatorUpdates = new List<ActuatorTelemetryDto>();
                foreach (var actuator in actuators)
                {
                    if (actuatorStates.TryGetValue(actuator.Id, out var state))
                    {
                        actuatorUpdates.Add(new ActuatorTelemetryDto
                        {
                            ActuatorId = actuator.Id,
                            Code = actuator.Code,
                            State = state
                        });
                    }
                }

                var update = new TelemetryUpdateDto
                {
                    Sensors = sensorUpdates,
                    Actuators = actuatorUpdates,
                    Timestamp = now
                };

                // Broadcast to all connected clients
                await _hubContext.Clients.All.TelemetryUpdate(update);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting telemetry");
            }

            await Task.Delay(TimeSpan.FromSeconds(_broadcastIntervalSeconds), stoppingToken);
        }
    }

    private static string DetermineStatus(decimal value, Domain.Entities.Sensor sensor)
    {
        if (value < sensor.CriticalThresholdLow || value > sensor.CriticalThresholdHigh)
            return "Critical";
        if (value < sensor.WarningThresholdLow || value > sensor.WarningThresholdHigh)
            return "Warning";
        return "Normal";
    }
}
