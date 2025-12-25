namespace SmokeSoft.Shared.Common;

/// <summary>
/// Represents the result of an operation
/// </summary>
/// <typeparam name="T">The type of the result value</typeparam>
public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? ErrorCode { get; private set; }
    public Dictionary<string, string[]>? ValidationErrors { get; private set; }

    private Result(bool isSuccess, T? data, string? errorMessage, string? errorCode, Dictionary<string, string[]>? validationErrors = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        ValidationErrors = validationErrors;
    }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static Result<T> Success(T data)
    {
        return new Result<T>(true, data, null, null);
    }

    /// <summary>
    /// Creates a failed result
    /// </summary>
    public static Result<T> Failure(string errorMessage, string? errorCode = null)
    {
        return new Result<T>(false, default, errorMessage, errorCode);
    }

    /// <summary>
    /// Creates a failed result with validation errors
    /// </summary>
    public static Result<T> ValidationFailure(Dictionary<string, string[]> validationErrors)
    {
        return new Result<T>(false, default, "Validation failed", "VALIDATION_ERROR", validationErrors);
    }
}

/// <summary>
/// Represents the result of an operation without a return value
/// </summary>
public class Result
{
    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? ErrorCode { get; private set; }
    public Dictionary<string, string[]>? ValidationErrors { get; private set; }

    private Result(bool isSuccess, string? errorMessage, string? errorCode, Dictionary<string, string[]>? validationErrors = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        ValidationErrors = validationErrors;
    }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static Result Success()
    {
        return new Result(true, null, null);
    }

    /// <summary>
    /// Creates a failed result
    /// </summary>
    public static Result Failure(string errorMessage, string? errorCode = null)
    {
        return new Result(false, errorMessage, errorCode);
    }

    /// <summary>
    /// Creates a failed result with validation errors
    /// </summary>
    public static Result ValidationFailure(Dictionary<string, string[]> validationErrors)
    {
        return new Result(false, "Validation failed", "VALIDATION_ERROR", validationErrors);
    }
}
