namespace Flowingly.ParsingService.Domain.Exceptions;

/// <summary>
/// Exception thrown when validation fails (e.g., unclosed or overlapping tags).
/// </summary>
public class ValidationException : Exception
{
    public string ErrorCode { get; }

    public ValidationException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}
