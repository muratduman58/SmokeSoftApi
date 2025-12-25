using SmokeSoft.Services.ShadowGuard.Data.Entities;
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
    
    // WebSocket Session Management
    Task<ConversationSession> CreateSessionAsync(Guid conversationId, string sessionId, string webSocketId);
    Task UpdateSessionMetricsAsync(string sessionId, int bytesProcessed, bool sent);
    Task CloseConversationAsync(Guid conversationId, string sessionId, string endReason);
    Task<Conversation?> GetByIdAsync(Guid conversationId, Guid userId);
}
