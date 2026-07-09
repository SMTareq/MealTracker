using MealExpenseTracker.Api.Data;
using MealExpenseTracker.Api.DTOs;
using MealExpenseTracker.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MealExpenseTracker.Api.Services;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse?> RefreshAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public AuthService(AppDbContext db, ITokenService tokenService, IOptions<JwtSettings> jwtSettings)
    {
        _db = db;
        _tokenService = tokenService;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var emailExists = await _db.Users.AnyAsync(u => u.Email == request.Email);
        if (emailExists) return null;

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.User, // Admins are promoted manually or via seed, not self-registered
            IsActive = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return await IssueTokensAsync(user);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user is null || !user.IsActive) return null;

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) return null;

        return await IssueTokensAsync(user);
    }

    public async Task<AuthResponse?> RefreshAsync(string refreshToken)
    {
        var existingToken = await _db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (existingToken is null || existingToken.IsRevoked || existingToken.ExpiresAt < DateTime.UtcNow)
            return null;

        if (existingToken.User is null || !existingToken.User.IsActive) return null;

        // Rotate: revoke old, issue new
        existingToken.IsRevoked = true;
        await _db.SaveChangesAsync();

        return await IssueTokensAsync(existingToken.User);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var existingToken = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
        if (existingToken is null) return;

        existingToken.IsRevoked = true;
        await _db.SaveChangesAsync();
    }

    private async Task<AuthResponse> IssueTokensAsync(User user)
    {
        var (accessToken, expiresAt) = _tokenService.GenerateAccessToken(user);
        var refreshTokenValue = _tokenService.GenerateRefreshToken();

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays)
        });

        await _db.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            AccessTokenExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            }
        };
    }
}
