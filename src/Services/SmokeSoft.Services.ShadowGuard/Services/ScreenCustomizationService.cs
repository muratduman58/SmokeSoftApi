using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using SmokeSoft.Services.ShadowGuard.Data;
using SmokeSoft.Services.ShadowGuard.Data.Entities;
using SmokeSoft.Shared.Common;
using SmokeSoft.Shared.Constants;
using SmokeSoft.Shared.DTOs.ShadowGuard;

namespace SmokeSoft.Services.ShadowGuard.Services;

public class ScreenCustomizationService : IScreenCustomizationService
{
    private readonly ShadowGuardDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private const string UploadFolder = "uploads/screenshots";

    public ScreenCustomizationService(
        ShadowGuardDbContext context,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _context = context;
        _environment = environment;
        _configuration = configuration;
    }

    public async Task<Result<ScreenCustomizationDto>> UploadScreenshotAsync(
        UploadScreenshotRequest request,
        Stream fileStream,
        string fileName,
        string contentType,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        // Validate file
        if (fileStream == null || fileStream.Length == 0)
        {
            return Result<ScreenCustomizationDto>.Failure("File is empty", ErrorCodes.VALIDATION_ERROR);
        }

        // Validate file size (max 10MB)
        if (fileStream.Length > 10 * 1024 * 1024)
        {
            return Result<ScreenCustomizationDto>.Failure("File size exceeds 10MB limit", ErrorCodes.VALIDATION_ERROR);
        }

        // Validate device ID format
        if (!Helpers.DeviceIdValidator.IsValid(request.DeviceId))
        {
            var error = Helpers.DeviceIdValidator.GetValidationError(request.DeviceId);
            return Result<ScreenCustomizationDto>.Failure(error, ErrorCodes.VALIDATION_ERROR);
        }

        // Validate content type
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(contentType.ToLower()))
        {
            return Result<ScreenCustomizationDto>.Failure(
                "Invalid file type. Only JPEG, PNG, and WebP are allowed",
                ErrorCodes.VALIDATION_ERROR
            );
        }

        // Validate magic bytes (ensure file is actually an image)
        if (!await Helpers.ImageValidator.IsValidImageAsync(fileStream, contentType))
        {
            return Result<ScreenCustomizationDto>.Failure(
                "File is not a valid image. " + Helpers.ImageValidator.GetSupportedFormatsMessage(),
                ErrorCodes.VALIDATION_ERROR
            );
        }

        // Validate screen type
        var validScreenTypes = new[] { "IncomingCall", "Conversation" };
        if (!validScreenTypes.Contains(request.ScreenType))
        {
            return Result<ScreenCustomizationDto>.Failure(
                "Invalid screen type. Must be 'IncomingCall' or 'Conversation'",
                ErrorCodes.VALIDATION_ERROR
            );
        }

        try
        {
            // Calculate file hash for duplicate detection
            var fileHash = await CalculateFileHashAsync(fileStream);
            fileStream.Position = 0; // Reset stream position after hash calculation

            // Check for duplicate: same device model, platform, platform version, screen type, and file hash
            var duplicate = await _context.ScreenCustomizations
                .FirstOrDefaultAsync(
                    sc => sc.DeviceModel == request.DeviceModel &&
                          sc.Platform == request.Platform &&
                          sc.PlatformVersion == request.PlatformVersion &&
                          sc.ScreenType == request.ScreenType &&
                          sc.FileHash == fileHash,
                    cancellationToken
                );

            if (duplicate != null)
            {
                // Duplicate found - return existing screenshot without saving new file
                return Result<ScreenCustomizationDto>.Success(MapToDto(duplicate));
            }

            // Create upload directory if not exists
            var uploadPath = Path.Combine(_environment.ContentRootPath, UploadFolder);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Generate unique filename
            var fileExtension = Path.GetExtension(fileName);
            var uniqueFileName = $"{request.DeviceId}_{request.ScreenType}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadPath, uniqueFileName);
            var relativePath = Path.Combine(UploadFolder, uniqueFileName).Replace("\\", "/");

            // Save file
            using (var fileStreamOut = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOut, cancellationToken);
            }

            // Create new record (no more updating/deleting old files)
            var screenshot = new ScreenCustomization
            {
                UserId = userId,
                DeviceId = request.DeviceId,
                ScreenType = request.ScreenType,
                ImagePath = relativePath,
                OriginalFileName = fileName,
                FileSizeBytes = fileStream.Length,
                ContentType = contentType,
                FileHash = fileHash,
                DeviceName = request.DeviceName,
                DeviceModel = request.DeviceModel,
                Platform = request.Platform,
                PlatformVersion = request.PlatformVersion,
                AppVersion = request.AppVersion
            };

            _context.ScreenCustomizations.Add(screenshot);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<ScreenCustomizationDto>.Success(MapToDto(screenshot));
        }
        catch (Exception ex)
        {
            return Result<ScreenCustomizationDto>.Failure(
                $"Failed to upload screenshot: {ex.Message}",
                ErrorCodes.OPERATION_FAILED
            );
        }
    }

    public async Task<Result<List<ScreenCustomizationListDto>>> GetDeviceScreenshotsAsync(
        string deviceId,
        CancellationToken cancellationToken = default)
    {
        var screenshots = await _context.ScreenCustomizations
            .Where(sc => sc.DeviceId == deviceId)
            .OrderByDescending(sc => sc.CreatedAt)
            .Select(sc => new ScreenCustomizationListDto
            {
                ScreenType = sc.ScreenType,
                ImageUrl = $"/api/screenshots/{sc.Id}",
                UploadedAt = sc.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result<List<ScreenCustomizationListDto>>.Success(screenshots);
    }

    public async Task<Result<ScreenCustomizationDto>> GetScreenshotByTypeAsync(
        string deviceId,
        string screenType,
        CancellationToken cancellationToken = default)
    {
        var screenshot = await _context.ScreenCustomizations
            .FirstOrDefaultAsync(
                sc => sc.DeviceId == deviceId && sc.ScreenType == screenType,
                cancellationToken
            );

        if (screenshot == null)
        {
            return Result<ScreenCustomizationDto>.Failure(
                "Screenshot not found",
                ErrorCodes.NOT_FOUND
            );
        }

        return Result<ScreenCustomizationDto>.Success(MapToDto(screenshot));
    }

    public async Task<Result> DeleteScreenshotAsync(
        Guid id,
        Guid? userId,
        CancellationToken cancellationToken = default)
    {
        var screenshot = await _context.ScreenCustomizations
            .FirstOrDefaultAsync(sc => sc.Id == id, cancellationToken);

        if (screenshot == null)
        {
            return Result.Failure("Screenshot not found", ErrorCodes.NOT_FOUND);
        }

        // Check ownership if user is provided
        if (userId.HasValue && screenshot.UserId != userId.Value)
        {
            return Result.Failure("Unauthorized", ErrorCodes.FORBIDDEN);
        }

        // Delete file
        var filePath = Path.Combine(_environment.ContentRootPath, screenshot.ImagePath);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        _context.ScreenCustomizations.Remove(screenshot);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> LinkScreenshotsToUserAsync(
        string deviceId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var screenshots = await _context.ScreenCustomizations
            .Where(sc => sc.DeviceId == deviceId && sc.UserId == null)
            .ToListAsync(cancellationToken);

        foreach (var screenshot in screenshots)
        {
            screenshot.UserId = userId;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<string> CalculateFileHashAsync(Stream stream)
    {
        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(stream);
        return Convert.ToBase64String(hash);
    }

    private ScreenCustomizationDto MapToDto(ScreenCustomization screenshot)
    {
        return new ScreenCustomizationDto
        {
            Id = screenshot.Id,
            UserId = screenshot.UserId,
            DeviceId = screenshot.DeviceId,
            ScreenType = screenshot.ScreenType,
            ImageUrl = $"/api/screenshots/{screenshot.Id}",
            OriginalFileName = screenshot.OriginalFileName,
            FileSizeBytes = screenshot.FileSizeBytes,
            DeviceName = screenshot.DeviceName,
            DeviceModel = screenshot.DeviceModel,
            Platform = screenshot.Platform,
            CreatedAt = screenshot.CreatedAt
        };
    }

    public async Task<Result<(Stream FileStream, string ContentType, string FileName)>> GetScreenshotFileAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var screenshot = await _context.ScreenCustomizations
            .FirstOrDefaultAsync(sc => sc.Id == id, cancellationToken);

        if (screenshot == null)
        {
            return Result<(Stream, string, string)>.Failure("Screenshot not found", ErrorCodes.NOT_FOUND);
        }

        var filePath = Path.Combine(_environment.ContentRootPath, screenshot.ImagePath);
        if (!File.Exists(filePath))
        {
            return Result<(Stream, string, string)>.Failure("File not found", ErrorCodes.NOT_FOUND);
        }

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Result<(Stream, string, string)>.Success((fileStream, screenshot.ContentType, screenshot.OriginalFileName));
    }
}
