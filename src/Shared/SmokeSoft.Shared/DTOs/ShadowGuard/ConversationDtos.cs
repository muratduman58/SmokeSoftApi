namespace SmokeSoft.Shared.DTOs.ShadowGuard;

public class StartConversationRequest
{
    public Guid AIIdentityId { get; set; }
}

public class SendMessageRequest
{
    public Guid ConversationId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class ConversationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid AIIdentityId { get; set; }
    public string AIIdentityName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public bool IsActive => EndedAt == null;
    public List<MessageDto> Messages { get; set; } = new();
}

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsFromUser { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ConversationListDto
{
    public Guid Id { get; set; }
    public Guid AIIdentityId { get; set; }
    public string AIIdentityName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public bool IsActive => EndedAt == null;
    public int MessageCount { get; set; }
    public MessageDto? LastMessage { get; set; }
}

// WebSocket message types
public class WebSocketMessage
{
    public string Type { get; set; } = string.Empty; // "message", "typing", "status", "error"
    public object? Data { get; set; }
}

public class ChatMessage
{
    public Guid ConversationId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsFromUser { get; set; }
    public DateTime Timestamp { get; set; }
}

public class TypingIndicator
{
    public Guid ConversationId { get; set; }
    public bool IsTyping { get; set; }
}

public class ConnectionStatus
{
    public bool IsConnected { get; set; }
    public string? Message { get; set; }
}
