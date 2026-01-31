namespace Pincho.Exceptions;

/// <summary>
/// Base exception for all Pincho Client Library errors.
/// </summary>
public class PinchoException : Exception
{
    /// <summary>
    /// Gets the HTTP status code associated with this exception, or 0 if not applicable.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Gets a value indicating whether this exception represents a retryable error.
    /// </summary>
    /// <remarks>
    /// Retryable errors include network failures, server errors (5xx), and rate limits (429).
    /// Non-retryable errors include validation errors (400) and authentication failures (401, 403).
    /// </remarks>
    public virtual bool IsRetryable { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PinchoException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public PinchoException(string message)
        : base(message)
    {
        StatusCode = 0;
        IsRetryable = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PinchoException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    public PinchoException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
        IsRetryable = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PinchoException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public PinchoException(string message, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = 0;
        IsRetryable = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PinchoException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="innerException">The inner exception.</param>
    public PinchoException(string message, int statusCode, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        IsRetryable = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PinchoException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="isRetryable">Whether this error is retryable.</param>
    protected PinchoException(string message, int statusCode, bool isRetryable)
        : base(message)
    {
        StatusCode = statusCode;
        IsRetryable = isRetryable;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PinchoException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="isRetryable">Whether this error is retryable.</param>
    /// <param name="innerException">The inner exception.</param>
    protected PinchoException(string message, int statusCode, bool isRetryable, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        IsRetryable = isRetryable;
    }
}
