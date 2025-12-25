using SmokeSoft.Shared.Common;
using SmokeSoft.Shared.DTOs.ShadowGuard;

namespace SmokeSoft.Services.ShadowGuard.Services;

public interface IConversationService
{
    Task<Result<PagedResult<ConversationListDto>>> GetUserConversationsAsync(Guid userId, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<Result<ConversationDto>> GetConversationByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<Result<ConversationDto>> StartConversationAsync(Guid userId, StartConversationRequest request, CancellationToken cancellationToken = default);
    Task<Result<MessageDto>> SendMessageAsync(Guid userId, SendMessageRequest request, CancellationToken cancellationToken = default);
    Task<Result> EndConversationAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}
