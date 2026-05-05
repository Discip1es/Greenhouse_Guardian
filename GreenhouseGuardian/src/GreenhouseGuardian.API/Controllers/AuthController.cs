using GreenhouseGuardian.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GreenhouseGuardian.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var (accessToken, refreshToken) = await _authService.LoginAsync(request.Email, request.Password);
            var user = await _authService.GetUserByEmailAsync(request.Email);
            
            if (user == null)
                return Unauthorized(new { Error = "Invalid credentials" });

            return Ok(new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role.ToString()
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Login failed for {Email}", request.Email);
            return Unauthorized(new { Error = ex.Message });
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var (accessToken, refreshToken) = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { Error = ex.Message });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await _authService.LogoutAsync(userId);
        }
        return Ok(new { Success = true });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound();

        return Ok(new
        {
            user.Id,
            user.Email,
            user.UserName,
            Role = user.Role.ToString(),
            user.IsActive,
            user.CreatedAt,
            user.LastLoginAt
        });
    }
}
