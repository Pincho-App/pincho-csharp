namespace WirePusher.Exceptions;

/// <summary>
/// Exception thrown when request validation fails (400/404 errors).
/// </summary>
public class ValidationException : WirePusherException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code (400 or 404).</param>
    public ValidationException(string message, int statusCode)
        : base(message, statusCode)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code (400 or 404).</param>
    /// <param name="innerException">The inner exception.</param>
    public ValidationException(string message, int statusCode, Exception innerException)
        : base(message, statusCode, innerException)
    {
    }
}
