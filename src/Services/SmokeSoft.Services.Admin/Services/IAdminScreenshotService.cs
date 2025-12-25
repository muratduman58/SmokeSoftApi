using SmokeSoft.Shared.Common;
using SmokeSoft.Shared.DTOs.Admin;

namespace SmokeSoft.Services.Admin.Services;

public interface IAdminScreenshotService
{
    Task<Result<PagedResult<AdminScreenshotDto>>> GetScreenshotsAsync(
        AdminScreenshotFilterDto filter,
        CancellationToken cancellationToken = default);
    
    Task<Result<AdminScreenshotDto>> GetScreenshotByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
    
    Task<Result<ScreenshotStatsDto>> GetStatsAsync(
        CancellationToken cancellationToken = default);
    
    Task<Result> DeleteScreenshotAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
