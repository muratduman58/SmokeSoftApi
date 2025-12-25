using SmokeSoft.Shared.Common;
using SmokeSoft.Shared.DTOs.ShadowGuard;

namespace SmokeSoft.Services.ShadowGuard.Services;

public interface IScreenCustomizationService
{
    Task<Result<ScreenCustomizationDto>> UploadScreenshotAsync(
        UploadScreenshotRequest request,
        Stream fileStream,
        string fileName,
        string contentType,
        Guid? userId = null,
        CancellationToken cancellationToken = default);

    Task<Result<List<ScreenCustomizationListDto>>> GetDeviceScreenshotsAsync(
        string deviceId,
        CancellationToken cancellationToken = default);

    Task<Result<ScreenCustomizationDto>> GetScreenshotByTypeAsync(
        string deviceId,
        string screenType,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteScreenshotAsync(
        Guid id,
        Guid? userId,
        CancellationToken cancellationToken = default);

    Task<Result> LinkScreenshotsToUserAsync(
        string deviceId,
        Guid userId,
        CancellationToken cancellationToken = default);
}
