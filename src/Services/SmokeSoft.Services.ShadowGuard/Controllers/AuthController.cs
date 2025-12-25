using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmokeSoft.Services.ShadowGuard.Services;
using SmokeSoft.Shared.DTOs.Auth;

namespace SmokeSoft.Services.ShadowGuard.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;
    private readonly IOAuthService _oauthService;
    private readonly IDeviceService _deviceService;

    public AuthController(
        IAuthService authService,
        IOAuthService oauthService,
        IDeviceService deviceService)
    {
        _authService = authService;
        _oauthService = oauthService;
        _deviceService = deviceService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    [HttpPost("oauth/login")]
    [AllowAnonymous]
    public async Task<IActionResult> OAuthLogin([FromBody] OAuthLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _oauthService.LoginWithOAuthAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _authService.LogoutAsync(userId, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "Logged out successfully" });
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _authService.GetCurrentUserAsync(userId, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _authService.UpdateProfileAsync(userId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _authService.ChangePasswordAsync(userId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "Password changed successfully" });
    }

    // OAuth Provider Management
    [HttpGet("oauth/providers")]
    public async Task<IActionResult> GetOAuthProviders(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _oauthService.GetUserOAuthProvidersAsync(userId, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    [HttpDelete("oauth/providers/{provider}")]
    public async Task<IActionResult> UnlinkOAuthProvider(string provider, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _oauthService.UnlinkOAuthProviderAsync(userId, provider, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = $"{provider} unlinked successfully" });
    }

    // Device Management
    [HttpPost("device/register")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterDevice([FromBody] DeviceInfoRequest request, CancellationToken cancellationToken)
    {
        // Device can be registered without user (anonymous)
        var result = await _deviceService.RegisterOrUpdateDeviceAsync(request, null, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    [HttpGet("devices")]
    public async Task<IActionResult> GetUserDevices(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _deviceService.GetUserDevicesAsync(userId, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    [HttpDelete("devices/{deviceId}")]
    public async Task<IActionResult> DeactivateDevice(Guid deviceId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _deviceService.DeactivateDeviceAsync(deviceId, userId, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "Device deactivated successfully" });
    }
}
