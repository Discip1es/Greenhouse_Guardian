using System.Net.Http.Json;
using GreenhouseGuardian.Web.Models;

namespace GreenhouseGuardian.Web.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;

    public ApiService(HttpClient httpClient, AuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    // Sensors
    public async Task<List<SensorViewModel>> GetSensorsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<SensorViewModel>>("api/sensors") ?? new();
    }

    public async Task<SensorViewModel?> GetSensorByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<SensorViewModel>($"api/sensors/{id}");
    }

    public async Task<SensorViewModel> CreateSensorAsync(SensorViewModel sensor)
    {
        var response = await _httpClient.PostAsJsonAsync("api/sensors", sensor);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SensorViewModel>() ?? throw new Exception("Failed to create sensor");
    }

    public async Task UpdateSensorAsync(SensorViewModel sensor)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/sensors/{sensor.Id}", sensor);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteSensorAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/sensors/{id}");
        response.EnsureSuccessStatusCode();
    }

    // Actuators
    public async Task<List<ActuatorViewModel>> GetActuatorsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<ActuatorViewModel>>("api/actuators") ?? new();
    }

    public async Task<ActuatorViewModel> ExecuteCommandAsync(int actuatorId, string action)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/actuators/{actuatorId}/execute", new { action });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ActuatorViewModel>() ?? throw new Exception("Failed to execute command");
    }

    // Zones
    public async Task<List<ZoneViewModel>> GetZonesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<ZoneViewModel>>("api/zones") ?? new();
    }

    public async Task<ZoneViewModel> CreateZoneAsync(ZoneViewModel zone)
    {
        var response = await _httpClient.PostAsJsonAsync("api/zones", zone);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ZoneViewModel>() ?? throw new Exception("Failed to create zone");
    }

    public async Task UpdateZoneAsync(ZoneViewModel zone)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/zones/{zone.Id}", zone);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteZoneAsync(string id)
    {
        var response = await _httpClient.DeleteAsync($"api/zones/{id}");
        response.EnsureSuccessStatusCode();
    }

    // Alerts
    public async Task<List<AlertViewModel>> GetAlertsAsync(bool? isAcknowledged = null)
    {
        var url = "api/alerts";
        if (isAcknowledged.HasValue)
            url += $"?isAcknowledged={isAcknowledged.Value}";
        return await _httpClient.GetFromJsonAsync<List<AlertViewModel>>(url) ?? new();
    }

    public async Task AcknowledgeAlertAsync(long alertId)
    {
        var response = await _httpClient.PostAsync($"api/alerts/{alertId}/acknowledge", null);
        response.EnsureSuccessStatusCode();
    }

    // Rules
    public async Task<List<RuleViewModel>> GetRulesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<RuleViewModel>>("api/rules") ?? new();
    }

    public async Task<RuleViewModel> CreateRuleAsync(RuleViewModel rule)
    {
        var response = await _httpClient.PostAsJsonAsync("api/rules", rule);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RuleViewModel>() ?? throw new Exception("Failed to create rule");
    }

    public async Task UpdateRuleAsync(RuleViewModel rule)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/rules/{rule.Id}", rule);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteRuleAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/rules/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task PauseRuleAsync(int id)
    {
        var response = await _httpClient.PostAsync($"api/rules/{id}/pause", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task ResumeRuleAsync(int id)
    {
        var response = await _httpClient.PostAsync($"api/rules/{id}/resume", null);
        response.EnsureSuccessStatusCode();
    }

    // Dashboard Stats
    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        return await _httpClient.GetFromJsonAsync<DashboardStats>("api/dashboard/stats") ?? new DashboardStats();
    }
}
