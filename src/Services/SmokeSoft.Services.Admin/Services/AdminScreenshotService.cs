using Microsoft.EntityFrameworkCore;
using SmokeSoft.Services.ShadowGuard.Data;
using SmokeSoft.Shared.Common;
using SmokeSoft.Shared.Constants;
using SmokeSoft.Shared.DTOs.Admin;

namespace SmokeSoft.Services.Admin.Services;

public class AdminScreenshotService : IAdminScreenshotService
{
    private readonly ShadowGuardDbContext _context;

    public AdminScreenshotService(ShadowGuardDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<AdminScreenshotDto>>> GetScreenshotsAsync(
        AdminScreenshotFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ScreenCustomizations
            .Include(sc => sc.User)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(filter.ScreenType))
        {
            query = query.Where(sc => sc.ScreenType == filter.ScreenType);
        }

        if (!string.IsNullOrEmpty(filter.Platform))
        {
            query = query.Where(sc => sc.Platform == filter.Platform);
        }

        if (filter.HasUser.HasValue)
        {
            if (filter.HasUser.Value)
            {
                query = query.Where(sc => sc.UserId != null);
            }
            else
            {
                query = query.Where(sc => sc.UserId == null);
            }
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var screenshots = await query
            .OrderByDescending(sc => sc.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(sc => new AdminScreenshotDto
            {
                Id = sc.Id,
                ScreenType = sc.ScreenType,
                ImagePath = sc.ImagePath,
                ImageUrl = $"/api/screenshots/{sc.Id}",
                FileSizeBytes = sc.FileSizeBytes,
                CreatedAt = sc.CreatedAt,
                DeviceId = sc.DeviceId,
                DeviceName = sc.DeviceName,
                DeviceModel = sc.DeviceModel,
                Platform = sc.Platform,
                PlatformVersion = sc.PlatformVersion,
                AppVersion = sc.AppVersion,
                HasUser = sc.UserId != null,
                UserId = sc.UserId,
                UserEmail = sc.User != null ? sc.User.Email : null,
                UserType = sc.UserId != null ? "Registered" : "Guest"
            })
            .ToListAsync(cancellationToken);

        var result = new PagedResult<AdminScreenshotDto>
        {
            Items = screenshots,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };

        return Result<PagedResult<AdminScreenshotDto>>.Success(result);
    }

    public async Task<Result<AdminScreenshotDto>> GetScreenshotByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var screenshot = await _context.ScreenCustomizations
            .Include(sc => sc.User)
            .Where(sc => sc.Id == id)
            .Select(sc => new AdminScreenshotDto
            {
                Id = sc.Id,
                ScreenType = sc.ScreenType,
                ImagePath = sc.ImagePath,
                ImageUrl = $"/api/screenshots/{sc.Id}",
                FileSizeBytes = sc.FileSizeBytes,
                CreatedAt = sc.CreatedAt,
                DeviceId = sc.DeviceId,
                DeviceName = sc.DeviceName,
                DeviceModel = sc.DeviceModel,
                Platform = sc.Platform,
                PlatformVersion = sc.PlatformVersion,
                AppVersion = sc.AppVersion,
                HasUser = sc.UserId != null,
                UserId = sc.UserId,
                UserEmail = sc.User != null ? sc.User.Email : null,
                UserType = sc.UserId != null ? "Registered" : "Guest"
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (screenshot == null)
        {
            return Result<AdminScreenshotDto>.Failure("Screenshot not found", ErrorCodes.NOT_FOUND);
        }

        return Result<AdminScreenshotDto>.Success(screenshot);
    }

    public async Task<Result<ScreenshotStatsDto>> GetStatsAsync(
        CancellationToken cancellationToken = default)
    {
        var screenshots = await _context.ScreenCustomizations
            .ToListAsync(cancellationToken);

        var stats = new ScreenshotStatsDto
        {
            TotalScreenshots = screenshots.Count,
            ByScreenType = screenshots
                .GroupBy(sc => sc.ScreenType)
                .ToDictionary(g => g.Key, g => g.Count()),
            ByPlatform = screenshots
                .GroupBy(sc => sc.Platform)
                .ToDictionary(g => g.Key, g => g.Count()),
            RegisteredUsers = screenshots.Count(sc => sc.UserId != null),
            GuestUsers = screenshots.Count(sc => sc.UserId == null)
        };

        return Result<ScreenshotStatsDto>.Success(stats);
    }

    public async Task<Result> DeleteScreenshotAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var screenshot = await _context.ScreenCustomizations
            .FirstOrDefaultAsync(sc => sc.Id == id, cancellationToken);

        if (screenshot == null)
        {
            return Result.Failure("Screenshot not found", ErrorCodes.NOT_FOUND);
        }

        // Delete physical file
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), screenshot.ImagePath);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        _context.ScreenCustomizations.Remove(screenshot);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
