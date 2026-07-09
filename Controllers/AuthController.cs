using MealExpenseTracker.Api.DTOs;
using MealExpenseTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MealExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (result is null) return Conflict(new { message = "Email is already registered." });
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result is null) return Unauthorized(new { message = "Invalid email or password." });
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request)
    {
        var result = await _authService.RefreshAsync(request.RefreshToken);
        if (result is null) return Unauthorized(new { message = "Invalid or expired refresh token." });
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshRequest request)
    {
        await _authService.RevokeRefreshTokenAsync(request.RefreshToken);
        return NoContent();
    }
}
