using Microsoft.EntityFrameworkCore;
using SmokeSoft.Services.ShadowGuard.Data;
using SmokeSoft.Services.ShadowGuard.Data.Entities;
using SmokeSoft.Shared.Common;
using SmokeSoft.Shared.Constants;
using SmokeSoft.Shared.DTOs.ShadowGuard;

namespace SmokeSoft.Services.ShadowGuard.Services;

public class ConversationService : IConversationService
{
    private readonly ShadowGuardDbContext _context;

    public ConversationService(ShadowGuardDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<ConversationListDto>>> GetUserConversationsAsync(
        Guid userId,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Conversations
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.StartedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(c => c.AIIdentity)
            .Include(c => c.Messages.OrderByDescending(m => m.Timestamp).Take(1))
            .Select(c => new ConversationListDto
            {
                Id = c.Id,
                AIIdentityId = c.AIIdentityId,
                AIIdentityName = c.AIIdentity.Name,
                StartedAt = c.StartedAt,
                EndedAt = c.EndedAt,
                MessageCount = c.Messages.Count,
                LastMessage = c.Messages.OrderByDescending(m => m.Timestamp).Select(m => new MessageDto
                {
                    Id = m.Id,
                    ConversationId = m.ConversationId,
                    Content = m.Content,
                    IsFromUser = m.IsFromUser,
                    Timestamp = m.Timestamp
                }).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<ConversationListDto>(items, totalCount, pageNumber, pageSize);

        return Result<PagedResult<ConversationListDto>>.Success(pagedResult);
    }

    public async Task<Result<ConversationDto>> GetConversationByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var conversation = await _context.Conversations
            .Include(c => c.AIIdentity)
            .Include(c => c.Messages.OrderBy(m => m.Timestamp))
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, cancellationToken);

        if (conversation == null)
        {
            return Result<ConversationDto>.Failure(
                "Conversation not found",
                ErrorCodes.CONVERSATION_NOT_FOUND
            );
        }

        var dto = new ConversationDto
        {
            Id = conversation.Id,
            UserId = conversation.UserId,
            AIIdentityId = conversation.AIIdentityId,
            AIIdentityName = conversation.AIIdentity.Name,
            StartedAt = conversation.StartedAt,
            EndedAt = conversation.EndedAt,
            Messages = conversation.Messages.Select(m => new MessageDto
            {
                Id = m.Id,
                ConversationId = m.ConversationId,
                Content = m.Content,
                IsFromUser = m.IsFromUser,
                Timestamp = m.Timestamp
            }).ToList()
        };

        return Result<ConversationDto>.Success(dto);
    }

    public async Task<Result<ConversationDto>> StartConversationAsync(
        Guid userId,
        StartConversationRequest request,
        CancellationToken cancellationToken = default)
    {
        // Verify AI Identity exists and belongs to user
        var aiIdentity = await _context.AIIdentities
            .FirstOrDefaultAsync(ai => ai.Id == request.AIIdentityId && ai.UserId == userId, cancellationToken);

        if (aiIdentity == null)
        {
            return Result<ConversationDto>.Failure(
                "AI Identity not found",
                ErrorCodes.AI_IDENTITY_NOT_FOUND
            );
        }

        var conversation = new Conversation
        {
            UserId = userId,
            AIIdentityId = request.AIIdentityId,
            StartedAt = DateTime.UtcNow
        };

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new ConversationDto
        {
            Id = conversation.Id,
            UserId = conversation.UserId,
            AIIdentityId = conversation.AIIdentityId,
            AIIdentityName = aiIdentity.Name,
            StartedAt = conversation.StartedAt,
            EndedAt = conversation.EndedAt,
            Messages = new List<MessageDto>()
        };

        return Result<ConversationDto>.Success(dto);
    }

    public async Task<Result<MessageDto>> SendMessageAsync(
        Guid userId,
        SendMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId && c.UserId == userId, cancellationToken);

        if (conversation == null)
        {
            return Result<MessageDto>.Failure(
                "Conversation not found",
                ErrorCodes.CONVERSATION_NOT_FOUND
            );
        }

        if (conversation.EndedAt != null)
        {
            return Result<MessageDto>.Failure(
                "Conversation has ended",
                ErrorCodes.CONVERSATION_ENDED
            );
        }

        var message = new Message
        {
            ConversationId = request.ConversationId,
            Content = request.Content,
            IsFromUser = true,
            Timestamp = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new MessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            Content = message.Content,
            IsFromUser = message.IsFromUser,
            Timestamp = message.Timestamp
        };

        return Result<MessageDto>.Success(dto);
    }

    public async Task<Result> EndConversationAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, cancellationToken);

        if (conversation == null)
        {
            return Result.Failure(
                "Conversation not found",
                ErrorCodes.CONVERSATION_NOT_FOUND
            );
        }

        conversation.EndedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
