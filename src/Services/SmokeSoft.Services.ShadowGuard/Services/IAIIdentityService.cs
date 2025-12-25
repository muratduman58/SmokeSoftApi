using SmokeSoft.Shared.Common;
using SmokeSoft.Shared.DTOs.ShadowGuard;

namespace SmokeSoft.Services.ShadowGuard.Services;

public interface IAIIdentityService
{
    Task<Result<PagedResult<AIIdentityDto>>> GetUserAIIdentitiesAsync(Guid userId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<Result<AIIdentityDto>> GetAIIdentityByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<Result<AIIdentityDto>> CreateAIIdentityAsync(Guid userId, CreateAIIdentityRequest request, CancellationToken cancellationToken = default);
    Task<Result<AIIdentityDto>> UpdateAIIdentityAsync(Guid id, Guid userId, UpdateAIIdentityRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAIIdentityAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}
