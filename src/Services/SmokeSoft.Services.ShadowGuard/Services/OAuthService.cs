using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SmokeSoft.Services.ShadowGuard.Configuration;
using SmokeSoft.Services.ShadowGuard.Data;
using SmokeSoft.Services.ShadowGuard.Data.Entities;
using SmokeSoft.Shared.Common;
using SmokeSoft.Shared.Constants;
using SmokeSoft.Shared.DTOs.Auth;

namespace SmokeSoft.Services.ShadowGuard.Services;

public class OAuthService : IOAuthService
{
    private readonly ShadowGuardDbContext _context;
    private readonly JwtSettings _jwtSettings;
    private readonly IDeviceService _deviceService;

    public OAuthService(
        ShadowGuardDbContext context,
        IOptions<JwtSettings> jwtSettings,
        IDeviceService deviceService)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
        _deviceService = deviceService;
    }

    public async Task<Result<OAuthAuthResponse>> LoginWithOAuthAsync(
        OAuthLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate and decode ID token
        var tokenInfo = await ValidateOAuthTokenAsync(request.Provider, request.IdToken);
        if (tokenInfo == null)
        {
            return Result<OAuthAuthResponse>.Failure(
                "Invalid OAuth token",
                ErrorCodes.INVALID_TOKEN
            );
        }

        // Check if OAuth provider is already linked
        var oauthProvider = await _context.UserOAuthProviders
            .Include(op => op.User)
                .ThenInclude(u => u.OAuthProviders)
            .FirstOrDefaultAsync(
                op => op.Provider == request.Provider && op.ProviderUserId == tokenInfo.UserId,
                cancellationToken
            );

        User user;
        bool isNewUser = false;

        if (oauthProvider != null)
        {
            // Existing user - update OAuth info
            user = oauthProvider.User;
            oauthProvider.Email = tokenInfo.Email;
            oauthProvider.Name = tokenInfo.Name;
            oauthProvider.ProfilePictureUrl = tokenInfo.Picture;
        }
        else
        {
            // Check if user exists with this email
            user = await _context.Users
                .Include(u => u.OAuthProviders)
                .FirstOrDefaultAsync(u => u.Email == tokenInfo.Email, cancellationToken);

            if (user == null)
            {
                // Create new user
                isNewUser = true;
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = tokenInfo.Email,
                    DisplayName = tokenInfo.Name,
                    IsActive = true,
                    TotalAIMinutes = 100,
                    TotalAISlots = 1
                };

                _context.Users.Add(user);

                // Create user profile
                var userProfile = new UserProfile
                {
                    UserId = user.Id
                };
                _context.UserProfiles.Add(userProfile);
            }

            // Link OAuth provider
            oauthProvider = new UserOAuthProvider
            {
                UserId = user.Id,
                Provider = request.Provider,
                ProviderUserId = tokenInfo.UserId,
                Email = tokenInfo.Email,
                Name = tokenInfo.Name,
                ProfilePictureUrl = tokenInfo.Picture,
                LinkedAt = DateTime.UtcNow
            };

            _context.UserOAuthProviders.Add(oauthProvider);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Register/update device and link to user
        await _deviceService.RegisterOrUpdateDeviceAsync(request.DeviceInfo, user.Id, cancellationToken);

        // Generate tokens
        var accessToken = GenerateAccessToken(user);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id, cancellationToken);

        // Get all linked providers
        var linkedProviders = user.OAuthProviders.Select(op => new OAuthProviderDto
        {
            Id = op.Id,
            Provider = op.Provider,
            Email = op.Email ?? string.Empty,
            Name = op.Name,
            ProfilePictureUrl = op.ProfilePictureUrl,
            LinkedAt = op.LinkedAt
        }).ToList();

        var response = new OAuthAuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            User = MapToUserDto(user),
            IsNewUser = isNewUser,
            LinkedProviders = linkedProviders
        };

        return Result<OAuthAuthResponse>.Success(response);
    }

    public async Task<Result<List<OAuthProviderDto>>> GetUserOAuthProvidersAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var providers = await _context.UserOAuthProviders
            .Where(op => op.UserId == userId)
            .Select(op => new OAuthProviderDto
            {
                Id = op.Id,
                Provider = op.Provider,
                Email = op.Email ?? string.Empty,
                Name = op.Name,
                ProfilePictureUrl = op.ProfilePictureUrl,
                LinkedAt = op.LinkedAt
            })
            .ToListAsync(cancellationToken);

        return Result<List<OAuthProviderDto>>.Success(providers);
    }

    public async Task<Result> UnlinkOAuthProviderAsync(
        Guid userId,
        string provider,
        CancellationToken cancellationToken = default)
    {
        var oauthProvider = await _context.UserOAuthProviders
            .FirstOrDefaultAsync(
                op => op.UserId == userId && op.Provider == provider,
                cancellationToken
            );

        if (oauthProvider == null)
        {
            return Result.Failure("OAuth provider not found", ErrorCodes.NOT_FOUND);
        }

        // Check if user has password or other OAuth providers
        var user = await _context.Users
            .Include(u => u.OAuthProviders)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user != null && string.IsNullOrEmpty(user.PasswordHash) && user.OAuthProviders.Count <= 1)
        {
            return Result.Failure(
                "Cannot unlink the only authentication method. Please set a password first.",
                ErrorCodes.FORBIDDEN
            );
        }

        _context.UserOAuthProviders.Remove(oauthProvider);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<OAuthTokenInfo?> ValidateOAuthTokenAsync(string provider, string idToken)
    {
        // TODO: Implement actual token validation with Google/Apple
        // For now, this is a placeholder that decodes the JWT without validation
        // In production, you should:
        // - For Google: Use Google.Apis.Auth library to validate
        // - For Apple: Validate using Apple's public keys

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(idToken);

            var userId = token.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var email = token.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var name = token.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            var givenName = token.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value;
            var familyName = token.Claims.FirstOrDefault(c => c.Type == "family_name")?.Value;
            var picture = token.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;

            if (string.IsNullOrEmpty(userId))
                return null;

            return new OAuthTokenInfo
            {
                UserId = userId,
                Email = email,
                Name = name,
                GivenName = givenName,
                FamilyName = familyName,
                Picture = picture
            };
        }
        catch
        {
            return null;
        }

        await Task.CompletedTask;
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

    private class OAuthTokenInfo
    {
        public string UserId { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? GivenName { get; set; }
        public string? FamilyName { get; set; }
        public string? Picture { get; set; }
    }
}
