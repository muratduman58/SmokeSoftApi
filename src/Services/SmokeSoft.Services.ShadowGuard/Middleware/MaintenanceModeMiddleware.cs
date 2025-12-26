using Microsoft.AspNetCore.Http;
using SmokeSoft.Services.ShadowGuard.Services;
using SmokeSoft.Shared.Models;

namespace SmokeSoft.Services.ShadowGuard.Middleware;

/// <summary>
/// Middleware to check if the system is in maintenance mode
/// Blocks all requests except admin endpoints when maintenance is active
/// </summary>
public class MaintenanceModeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MaintenanceModeMiddleware> _logger;

    public MaintenanceModeMiddleware(
        RequestDelegate next,
        ILogger<MaintenanceModeMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ISystemConfigService configService)
    {
        // Skip admin endpoints - admins can always access
        if (context.Request.Path.StartsWithSegments("/api/admin"))
        {
            await _next(context);
            return;
        }
        
        // Skip maintenance status endpoint - always accessible
        if (context.Request.Path.StartsWithSegments("/api/shadowguard/System/maintenance/status"))
        {
            await _next(context);
            return;
        }

        // Skip health check endpoint
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        // Skip Swagger in development
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        // Get system configuration (from cache)
        var config = await configService.GetConfigAsync();

        // Check if maintenance mode is active
        if (config.IsMaintenanceMode)
        {
            _logger.LogWarning(
                "Request blocked due to maintenance mode. Path: {Path}, IP: {IP}",
                context.Request.Path,
                context.Connection.RemoteIpAddress);

            context.Response.StatusCode = 503; // Service Unavailable
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.ErrorResult(
                "MAINTENANCE_MODE",
                config.MaintenanceMessage ?? "Sistem bakımda. Lütfen daha sonra tekrar deneyin.",
                config.MaintenanceStartedAt.HasValue
                    ? $"Bakım başlangıç: {config.MaintenanceStartedAt:yyyy-MM-dd HH:mm:ss} UTC"
                    : null
            );

            await context.Response.WriteAsJsonAsync(response);
            return;
        }

        // Continue to next middleware
        await _next(context);
    }
}

/// <summary>
/// Extension method to register MaintenanceModeMiddleware
/// </summary>
public static class MaintenanceModeMiddlewareExtensions
{
    public static IApplicationBuilder UseMaintenanceMode(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MaintenanceModeMiddleware>();
    }
}
