using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using SmokeSoft.Services.ShadowGuard.Configuration;
using SmokeSoft.Services.ShadowGuard.Data;
using SmokeSoft.Services.ShadowGuard.Data.Entities;
using SmokeSoft.Shared.Common;
using SmokeSoft.Shared.Constants;
using SmokeSoft.Shared.DTOs.Auth;

namespace SmokeSoft.Services.ShadowGuard.Services;

public class AuthService : IAuthService
{
    private readonly ShadowGuardDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthService(ShadowGuardDbContext context, IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // Validate email
        if (await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
        {
            return Result<AuthResponse>.Failure(
                "Email already exists",
                ErrorCodes.EMAIL_ALREADY_EXISTS
            );
        }

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DisplayName = $"{request.FirstName} {request.LastName}".Trim(),
            PhoneNumber = request.PhoneNumber,
            IsActive = true,
            TotalAIMinutes = 100, // Free tier: 100 minutes
            TotalAISlots = 1 // Free tier: 1 AI slot
        };
        _context.Users.Add(user);

        // Create user profile
        var userProfile = new UserProfile
        {
            UserId = user.Id
        };

        _context.UserProfiles.Add(userProfile);

        await _context.SaveChangesAsync(cancellationToken);

        // Generate tokens
        var accessToken = GenerateAccessToken(user);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id, cancellationToken);

        var response = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            User = MapToUserDto(user)
        };

        return Result<AuthResponse>.Success(response);
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Result<AuthResponse>.Failure(
                "Invalid email or password",
                ErrorCodes.INVALID_CREDENTIALS
            );
        }

        if (!user.IsActive)
        {
            return Result<AuthResponse>.Failure(
                "User account is inactive",
                ErrorCodes.FORBIDDEN
            );
        }

        // Generate tokens
        var accessToken = GenerateAccessToken(user);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id, cancellationToken);

        var response = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            User = MapToUserDto(user)
        };

        return Result<AuthResponse>.Success(response);
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (token == null || !token.IsActive)
        {
            return Result<AuthResponse>.Failure(
                "Invalid or expired refresh token",
                ErrorCodes.INVALID_TOKEN
            );
        }

        // Revoke old token
        token.RevokedAt = DateTime.UtcNow;

        // Generate new tokens
        var accessToken = GenerateAccessToken(token.User);
        var newRefreshToken = await GenerateRefreshTokenAsync(token.UserId, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        var response = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            User = MapToUserDto(token.User)
        };

        return Result<AuthResponse>.Success(response);
    }

    public async Task<Result> LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<UserDto>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.Failure(
                "User not found",
                ErrorCodes.USER_NOT_FOUND
            );
        }

        return Result<UserDto>.Success(MapToUserDto(user));
    }

    public async Task<Result<UserDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.Failure(
                "User not found",
                ErrorCodes.USER_NOT_FOUND
            );
        }

        // Update user profile
        user.DisplayName = $"{request.FirstName} {request.LastName}".Trim();
        user.PhoneNumber = request.PhoneNumber;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<UserDto>.Success(MapToUserDto(user));
    }

    public async Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return Result.Failure(
                "User not found",
                ErrorCodes.USER_NOT_FOUND
            );
        }

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return Result.Failure(
                "Current password is incorrect",
                ErrorCodes.INVALID_CREDENTIALS
            );
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, user.DisplayName ?? user.Email ?? user.Id.ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return refreshToken;
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            PhoneNumber = user.PhoneNumber,
            CreatedAt = user.CreatedAt
        };
    }
}
