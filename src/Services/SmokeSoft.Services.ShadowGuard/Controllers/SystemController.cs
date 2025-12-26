using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmokeSoft.Services.ShadowGuard.Services;

namespace SmokeSoft.Services.ShadowGuard.Controllers;

[ApiController]
[Route("api/shadowguard/[controller]")]
public class SystemController : BaseController
{
    private readonly ISystemConfigService _configService;
    
    public SystemController(ISystemConfigService configService)
    {
        _configService = configService;
    }
    
    /// <summary>
    /// Get maintenance mode status
    /// </summary>
    /// <remarks>
    /// This endpoint is used by the ShadowGuard mobile app to check if the system is in maintenance mode.
    /// No authentication required.
    /// </remarks>
    [HttpGet("maintenance/status")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMaintenanceStatus()
    {
        var config = await _configService.GetConfigAsync();
        
        return Success(new
        {
            isMaintenanceMode = config.IsMaintenanceMode,
            message = config.MaintenanceMessage,
            startedAt = config.MaintenanceStartedAt
        });
    }
    
    /// <summary>
    /// Invalidate system config cache
    /// </summary>
    /// <remarks>
    /// This endpoint is called by Admin API when system configuration changes.
    /// Only accessible from localhost (internal use).
    /// </remarks>
    [HttpPost("cache/invalidate")]
    [AllowAnonymous]
    public async Task<IActionResult> InvalidateCache()
    {
        // Security: Only allow requests from localhost
        var remoteIp = HttpContext.Connection.RemoteIpAddress;
        var isLocalhost = remoteIp != null && 
                         (remoteIp.ToString() == "127.0.0.1" || 
                          remoteIp.ToString() == "::1" || 
                          remoteIp.IsIPv4MappedToIPv6 && remoteIp.MapToIPv4().ToString() == "127.0.0.1");
        
        if (!isLocalhost)
        {
            return Forbid();
        }
        
        await _configService.InvalidateCacheAsync();
        return Ok(new { message = "Cache invalidated successfully" });
    }
}
