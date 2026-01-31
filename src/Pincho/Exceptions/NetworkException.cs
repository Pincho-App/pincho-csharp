namespace Pincho.Exceptions;

/// <summary>
/// Exception thrown when a network error occurs.
/// </summary>
/// <remarks>
/// Network exceptions are retryable. The client will automatically retry with exponential backoff.
/// This includes DNS failures, connection timeouts, and other network-level errors.
/// </remarks>
public class NetworkException : PinchoException
{
    /// <summary>
    /// Gets a value indicating whether this exception is retryable.
    /// </summary>
    /// <remarks>
    /// Network exceptions are always retryable with exponential backoff.
    /// </remarks>
    public override bool IsRetryable => true;

    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public NetworkException(string message)
        : base(message, 0, true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public NetworkException(string message, Exception innerException)
        : base(message, 0, true, innerException)
    {
    }
}
