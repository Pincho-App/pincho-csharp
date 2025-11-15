namespace WirePusher.Exceptions;

/// <summary>
/// Exception thrown when rate limit is exceeded (429 errors).
/// </summary>
/// <remarks>
/// Rate limit exceptions are retryable. The client will automatically retry with exponential backoff.
/// </remarks>
public class RateLimitException : WirePusherException
{
    /// <summary>
    /// Gets a value indicating whether this exception is retryable.
    /// </summary>
    /// <remarks>
    /// Rate limit exceptions are always retryable with exponential backoff.
    /// </remarks>
    public override bool IsRetryable => true;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code (429).</param>
    public RateLimitException(string message, int statusCode)
        : base(message, statusCode, true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code (429).</param>
    /// <param name="innerException">The inner exception.</param>
    public RateLimitException(string message, int statusCode, Exception innerException)
        : base(message, statusCode, true, innerException)
    {
    }
}
