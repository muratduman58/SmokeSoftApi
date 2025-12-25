using SmokeSoft.Shared.Common;
using SmokeSoft.Shared.DTOs.Auth;

namespace SmokeSoft.Services.ShadowGuard.Services;

public interface IOAuthService
{
    Task<Result<OAuthAuthResponse>> LoginWithOAuthAsync(OAuthLoginRequest request, CancellationToken cancellationToken = default);
    Task<Result<List<OAuthProviderDto>>> GetUserOAuthProvidersAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result> UnlinkOAuthProviderAsync(Guid userId, string provider, CancellationToken cancellationToken = default);
}
