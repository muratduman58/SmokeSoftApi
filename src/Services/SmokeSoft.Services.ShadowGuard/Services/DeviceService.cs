using Microsoft.EntityFrameworkCore;
using SmokeSoft.Services.ShadowGuard.Data;
using SmokeSoft.Services.ShadowGuard.Data.Entities;
using SmokeSoft.Shared.Common;
using SmokeSoft.Shared.Constants;
using SmokeSoft.Shared.DTOs.Auth;

namespace SmokeSoft.Services.ShadowGuard.Services;

public class DeviceService : IDeviceService
{
    private readonly ShadowGuardDbContext _context;

    public DeviceService(ShadowGuardDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DeviceDto>> RegisterOrUpdateDeviceAsync(
        DeviceInfoRequest request,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        // Check if device already exists
        var existingDevice = await _context.Devices
            .FirstOrDefaultAsync(d => d.DeviceId == request.DeviceId, cancellationToken);

        if (existingDevice != null)
        {
            // Update existing device
            existingDevice.DeviceName = request.DeviceName;
            existingDevice.DeviceModel = request.DeviceModel;
            existingDevice.Platform = request.Platform;
            existingDevice.PlatformVersion = request.PlatformVersion;
            existingDevice.AppVersion = request.AppVersion;
            existingDevice.FcmToken = request.FcmToken;
            existingDevice.LastSeenAt = DateTime.UtcNow;

            // Link to user if provided and not already linked
            if (userId.HasValue && existingDevice.UserId == null)
            {
                existingDevice.UserId = userId.Value;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<DeviceDto>.Success(MapToDto(existingDevice));
        }

        // Create new device
        var device = new Device
        {
            DeviceId = request.DeviceId,
            DeviceName = request.DeviceName,
            DeviceModel = request.DeviceModel,
            Platform = request.Platform,
            PlatformVersion = request.PlatformVersion,
            AppVersion = request.AppVersion,
            FcmToken = request.FcmToken,
            UserId = userId,
            LastSeenAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Devices.Add(device);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<DeviceDto>.Success(MapToDto(device));
    }

    public async Task<Result<List<DeviceDto>>> GetUserDevicesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var devices = await _context.Devices
            .Where(d => d.UserId == userId && d.IsActive)
            .OrderByDescending(d => d.LastSeenAt)
            .ToListAsync(cancellationToken);

        var deviceDtos = devices.Select(MapToDto).ToList();

        return Result<List<DeviceDto>>.Success(deviceDtos);
    }

    public async Task<Result> LinkDeviceToUserAsync(string deviceId, Guid userId, CancellationToken cancellationToken = default)
    {
        var device = await _context.Devices
            .FirstOrDefaultAsync(d => d.DeviceId == deviceId, cancellationToken);

        if (device == null)
        {
            return Result.Failure("Device not found", ErrorCodes.NOT_FOUND);
        }

        device.UserId = userId;
        device.LastSeenAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> UpdateDeviceLastSeenAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        var device = await _context.Devices
            .FirstOrDefaultAsync(d => d.DeviceId == deviceId, cancellationToken);

        if (device == null)
        {
            return Result.Failure("Device not found", ErrorCodes.NOT_FOUND);
        }

        device.LastSeenAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeactivateDeviceAsync(Guid deviceId, Guid userId, CancellationToken cancellationToken = default)
    {
        var device = await _context.Devices
            .FirstOrDefaultAsync(d => d.Id == deviceId && d.UserId == userId, cancellationToken);

        if (device == null)
        {
            return Result.Failure("Device not found", ErrorCodes.NOT_FOUND);
        }

        device.IsActive = false;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static DeviceDto MapToDto(Device device)
    {
        return new DeviceDto
        {
            Id = device.Id,
            DeviceId = device.DeviceId,
            DeviceName = device.DeviceName,
            DeviceModel = device.DeviceModel,
            Platform = device.Platform,
            PlatformVersion = device.PlatformVersion,
            AppVersion = device.AppVersion,
            LastSeenAt = device.LastSeenAt,
            IsActive = device.IsActive,
            CreatedAt = device.CreatedAt
        };
    }
}
