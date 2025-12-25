using SmokeSoft.Shared.Common;
using SmokeSoft.Shared.DTOs.Auth;

namespace SmokeSoft.Services.ShadowGuard.Services;

public interface IDeviceService
{
    Task<Result<DeviceDto>> RegisterOrUpdateDeviceAsync(DeviceInfoRequest request, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<Result<List<DeviceDto>>> GetUserDevicesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result> LinkDeviceToUserAsync(string deviceId, Guid userId, CancellationToken cancellationToken = default);
    Task<Result> UpdateDeviceLastSeenAsync(string deviceId, CancellationToken cancellationToken = default);
    Task<Result> DeactivateDeviceAsync(Guid deviceId, Guid userId, CancellationToken cancellationToken = default);
}
