namespace SmokeSoft.Shared.Models;

/// <summary>
/// Standard API response wrapper for all endpoints
/// </summary>
/// <typeparam name="T">Type of data being returned</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates whether the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Human-readable message about the operation result
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// The actual data payload (null if operation failed)
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Error details (null if operation succeeded)
    /// </summary>
    public ErrorDetail? Error { get; set; }

    /// <summary>
    /// Creates a successful response with data
    /// </summary>
    public static ApiResponse<T> SuccessResult(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message ?? "İşlem başarılı",
            Data = data,
            Error = null
        };
    }

    /// <summary>
    /// Creates an error response
    /// </summary>
    public static ApiResponse<T> ErrorResult(string code, string message, string? details = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Error = new ErrorDetail
            {
                Code = code,
                Details = details
            }
        };
    }
}

/// <summary>
/// Error details for failed operations
/// </summary>
public class ErrorDetail
{
    /// <summary>
    /// Machine-readable error code (e.g., "EMAIL_ALREADY_EXISTS")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Additional error details or context
    /// </summary>
    public string? Details { get; set; }
}
