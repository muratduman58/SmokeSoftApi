using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmokeSoft.Services.ShadowGuard.Services;
using SmokeSoft.Shared.DTOs.ShadowGuard;

namespace SmokeSoft.Services.ShadowGuard.Controllers;

[Route("api/screenshots")]
[ApiController]
public class ScreenshotController : BaseController
{
    private readonly IScreenCustomizationService _screenCustomizationService;

    public ScreenshotController(IScreenCustomizationService screenCustomizationService)
    {
        _screenCustomizationService = screenCustomizationService;
    }

    [HttpPost("upload")]
    [AllowAnonymous]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
    public async Task<IActionResult> UploadScreenshot(
        [FromForm] UploadScreenshotRequest request,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No file uploaded" });
        }

        // Try to get user ID if authenticated
        Guid? userId = null;
        try
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                userId = GetUserId();
            }
        }
        catch
        {
            // User not authenticated, continue without userId
        }

        using var stream = file.OpenReadStream();
        var result = await _screenCustomizationService.UploadScreenshotAsync(
            request,
            stream,
            file.FileName,
            file.ContentType,
            userId,
            cancellationToken
        );

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    [HttpGet("device/{deviceId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDeviceScreenshots(string deviceId, CancellationToken cancellationToken)
    {
        var result = await _screenCustomizationService.GetDeviceScreenshotsAsync(deviceId, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    [HttpGet("device/{deviceId}/{screenType}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetScreenshotByType(
        string deviceId,
        string screenType,
        CancellationToken cancellationToken)
    {
        var result = await _screenCustomizationService.GetScreenshotByTypeAsync(deviceId, screenType, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetScreenshotFile(Guid id, CancellationToken cancellationToken)
    {
        var service = _screenCustomizationService as ScreenCustomizationService;
        if (service == null)
        {
            return StatusCode(500, new { error = "Service not available" });
        }

        var result = await service.GetScreenshotFileAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.ErrorMessage });
        }

        var (fileStream, contentType, fileName) = result.Data!;
        return File(fileStream, contentType, fileName);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteScreenshot(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _screenCustomizationService.DeleteScreenshotAsync(id, userId, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        return Ok(new { message = "Screenshot deleted successfully" });
    }
}
