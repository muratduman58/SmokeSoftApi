using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SmokeSoft.Shared.Models;

namespace SmokeSoft.Services.ShadowGuard.Controllers;

[ApiController]
[Authorize]
public abstract class BaseController : ControllerBase
{
    protected Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }

        return userId;
    }

    protected string GetUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value 
               ?? User.FindFirst("email")?.Value 
               ?? throw new UnauthorizedAccessException("Email not found in token");
    }

    // ApiResponse Helper Methods
    
    /// <summary>
    /// Returns a successful response with data
    /// </summary>
    protected IActionResult Success<T>(T data, string? message = null)
    {
        return Ok(ApiResponse<T>.SuccessResult(data, message));
    }

    /// <summary>
    /// Returns an error response with 400 Bad Request
    /// </summary>
    protected IActionResult Error(string code, string message, string? details = null)
    {
        return BadRequest(ApiResponse<object>.ErrorResult(code, message, details));
    }

    /// <summary>
    /// Returns a not found error response with 404 Not Found
    /// </summary>
    protected IActionResult NotFoundError(string message = "Kayıt bulunamadı", string? details = null)
    {
        return NotFound(ApiResponse<object>.ErrorResult("NOT_FOUND", message, details));
    }

    /// <summary>
    /// Returns a validation error response with 400 Bad Request
    /// </summary>
    protected IActionResult ValidationError(string message, string? details = null)
    {
        return BadRequest(ApiResponse<object>.ErrorResult("VALIDATION_ERROR", message, details));
    }

    /// <summary>
    /// Returns an unauthorized error response with 401 Unauthorized
    /// </summary>
    protected IActionResult UnauthorizedError(string message = "Yetkisiz erişim", string? details = null)
    {
        return Unauthorized(ApiResponse<object>.ErrorResult("UNAUTHORIZED", message, details));
    }
}
