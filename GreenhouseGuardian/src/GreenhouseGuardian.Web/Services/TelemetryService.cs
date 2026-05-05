using Microsoft.AspNetCore.SignalR.Client;

namespace GreenhouseGuardian.Web.Services;

public class TelemetryService : IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly NavigationManager _navigationManager;

    public event Action? OnDataReceived;
    public Dictionary<int, decimal> SensorValues { get; set; } = new();
    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public TelemetryService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public async Task StartAsync()
    {
        if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
            return;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(new Uri(new Uri(_navigationManager.BaseUri), "telemetryHub"))
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<Dictionary<int, decimal>>("ReceiveTelemetry", (values) =>
        {
            SensorValues = values;
            OnDataReceived?.Invoke();
        });

        try
        {
            await _hubConnection.StartAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to telemetry hub: {ex.Message}");
        }
    }

    public async Task StopAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
