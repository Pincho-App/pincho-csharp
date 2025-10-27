namespace WirePusher.Exceptions;

/// <summary>
/// Exception thrown when rate limit is exceeded (429 errors).
/// </summary>
public class RateLimitException : WirePusherException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code (429).</param>
    public RateLimitException(string message, int statusCode)
        : base(message, statusCode)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code (429).</param>
    /// <param name="innerException">The inner exception.</param>
    public RateLimitException(string message, int statusCode, Exception innerException)
        : base(message, statusCode, innerException)
    {
    }
}
