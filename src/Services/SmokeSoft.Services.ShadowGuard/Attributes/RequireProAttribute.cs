using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using SmokeSoft.Services.ShadowGuard.Data;
using System.Security.Claims;

namespace SmokeSoft.Services.ShadowGuard.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequireProAttribute : TypeFilterAttribute
{
    public RequireProAttribute() : base(typeof(ProAuthorizationFilter)) { }
}

public class ProAuthorizationFilter : IAsyncActionFilter
{
    private readonly ShadowGuardDbContext _context;

    public ProAuthorizationFilter(ShadowGuardDbContext context)
    {
        _context = context;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        // Get userId from JWT
        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? context.HttpContext.User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                error = "UNAUTHORIZED",
                message = "Invalid or missing authentication token"
            });
            return;
        }

        // Check Pro status from database
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                error = "USER_NOT_FOUND",
                message = "User not found"
            });
            return;
        }

        if (!user.IsPro || user.ProExpiresAt < DateTime.UtcNow)
        {
            context.Result = new ObjectResult(new
            {
                error = "PRO_REQUIRED",
                message = "This feature requires an active Pro subscription",
                isPro = user.IsPro,
                expiresAt = user.ProExpiresAt
            })
            {
                StatusCode = 403
            };
            return;
        }

        await next();
    }
}
