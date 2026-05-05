using System.Net.Http.Json;
using GreenhouseGuardian.Web.Models;

namespace GreenhouseGuardian.Web.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private UserInfo? _currentUser;
    private string? _accessToken;

    public event Action? OnAuthStateChanged;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public UserInfo? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null;

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", new { email, password });
            if (!response.IsSuccessStatusCode)
                return false;

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (authResponse == null)
                return false;

            _accessToken = authResponse.AccessToken;
            _currentUser = authResponse.User;

            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

            OnAuthStateChanged?.Invoke();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Logout()
    {
        _currentUser = null;
        _accessToken = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
        OnAuthStateChanged?.Invoke();
    }

    public void SetToken(string token)
    {
        _accessToken = token;
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
}
