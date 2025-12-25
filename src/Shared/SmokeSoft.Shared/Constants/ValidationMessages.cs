namespace SmokeSoft.Shared.Constants;

/// <summary>
/// Validation messages used throughout the application
/// </summary>
public static class ValidationMessages
{
    // General
    public const string REQUIRED = "{0} is required";
    public const string INVALID_FORMAT = "{0} has an invalid format";
    public const string TOO_SHORT = "{0} must be at least {1} characters";
    public const string TOO_LONG = "{0} must not exceed {1} characters";
    public const string OUT_OF_RANGE = "{0} must be between {1} and {2}";

    // Email
    public const string INVALID_EMAIL = "Invalid email address";
    public const string EMAIL_REQUIRED = "Email is required";

    // Password
    public const string PASSWORD_REQUIRED = "Password is required";
    public const string PASSWORD_TOO_SHORT = "Password must be at least 8 characters";
    public const string PASSWORD_WEAK = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character";

    // Name
    public const string FIRST_NAME_REQUIRED = "First name is required";
    public const string LAST_NAME_REQUIRED = "Last name is required";

    // AI Identity
    public const string AI_NAME_REQUIRED = "AI identity name is required";
    public const string AI_NAME_TOO_LONG = "AI identity name must not exceed 100 characters";
    public const string GREETING_STYLE_REQUIRED = "Greeting style is required";
    public const string FORMALITY_OUT_OF_RANGE = "Formality must be between 0 and 100";
    public const string EMOTION_OUT_OF_RANGE = "Emotion must be between 0 and 100";
    public const string VERBOSITY_OUT_OF_RANGE = "Verbosity must be between 0 and 100";
    public const string SENSITIVITY_OUT_OF_RANGE = "Sensitivity level must be between 0 and 100";

    // Voice Recording
    public const string VOICE_FILE_REQUIRED = "Voice file is required";
    public const string VOICE_FILE_TOO_LARGE = "Voice file must not exceed 10MB";
    public const string INVALID_AUDIO_FORMAT = "Invalid audio format. Supported formats: mp3, wav, m4a";

    // Conversation
    public const string MESSAGE_REQUIRED = "Message content is required";
    public const string MESSAGE_TOO_LONG = "Message must not exceed 5000 characters";
}
