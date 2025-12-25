namespace SmokeSoft.Shared.Constants;

/// <summary>
/// Error codes used throughout the application
/// </summary>
public static class ErrorCodes
{
    // Authentication & Authorization
    public const string UNAUTHORIZED = "UNAUTHORIZED";
    public const string FORBIDDEN = "FORBIDDEN";
    public const string INVALID_CREDENTIALS = "INVALID_CREDENTIALS";
    public const string TOKEN_EXPIRED = "TOKEN_EXPIRED";
    public const string INVALID_TOKEN = "INVALID_TOKEN";
    public const string USER_NOT_FOUND = "USER_NOT_FOUND";
    public const string EMAIL_ALREADY_EXISTS = "EMAIL_ALREADY_EXISTS";
    public const string WEAK_PASSWORD = "WEAK_PASSWORD";

    // Validation
    public const string VALIDATION_ERROR = "VALIDATION_ERROR";
    public const string INVALID_INPUT = "INVALID_INPUT";
    public const string REQUIRED_FIELD = "REQUIRED_FIELD";

    // Resources
    public const string NOT_FOUND = "NOT_FOUND";
    public const string ALREADY_EXISTS = "ALREADY_EXISTS";
    public const string CONFLICT = "CONFLICT";

    // Operations
    public const string OPERATION_FAILED = "OPERATION_FAILED";
    public const string DATABASE_ERROR = "DATABASE_ERROR";
    public const string EXTERNAL_SERVICE_ERROR = "EXTERNAL_SERVICE_ERROR";

    // AI Identity
    public const string AI_IDENTITY_NOT_FOUND = "AI_IDENTITY_NOT_FOUND";
    public const string AI_IDENTITY_LIMIT_REACHED = "AI_IDENTITY_LIMIT_REACHED";
    public const string VOICE_RECORDING_FAILED = "VOICE_RECORDING_FAILED";
    public const string INVALID_VOICE_FORMAT = "INVALID_VOICE_FORMAT";

    // Conversation
    public const string CONVERSATION_NOT_FOUND = "CONVERSATION_NOT_FOUND";
    public const string CONVERSATION_ENDED = "CONVERSATION_ENDED";
    public const string MESSAGE_SEND_FAILED = "MESSAGE_SEND_FAILED";

    // WebSocket
    public const string WEBSOCKET_CONNECTION_FAILED = "WEBSOCKET_CONNECTION_FAILED";
    public const string WEBSOCKET_DISCONNECTED = "WEBSOCKET_DISCONNECTED";
    public const string WEBSOCKET_AUTH_FAILED = "WEBSOCKET_AUTH_FAILED";
}
