namespace Pincho.Exceptions;

/// <summary>
/// Exception thrown when the server returns a 5xx error.
/// </summary>
/// <remarks>
/// Server exceptions are retryable. The client will automatically retry with exponential backoff.
/// </remarks>
public class ServerException : PinchoException
{
    /// <summary>
    /// Gets a value indicating whether this exception is retryable.
    /// </summary>
    /// <remarks>
    /// Server exceptions are always retryable with exponential backoff.
    /// </remarks>
    public override bool IsRetryable => true;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code (5xx).</param>
    public ServerException(string message, int statusCode)
        : base(message, statusCode, true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code (5xx).</param>
    /// <param name="innerException">The inner exception.</param>
    public ServerException(string message, int statusCode, Exception innerException)
        : base(message, statusCode, true, innerException)
    {
    }
}
