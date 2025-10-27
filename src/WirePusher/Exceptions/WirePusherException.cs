namespace WirePusher.Exceptions;

/// <summary>
/// Base exception for all WirePusher SDK errors.
/// </summary>
public class WirePusherException : Exception
{
    /// <summary>
    /// Gets the HTTP status code associated with this exception, or 0 if not applicable.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WirePusherException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public WirePusherException(string message)
        : base(message)
    {
        StatusCode = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WirePusherException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    public WirePusherException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WirePusherException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public WirePusherException(string message, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WirePusherException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="innerException">The inner exception.</param>
    public WirePusherException(string message, int statusCode, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
