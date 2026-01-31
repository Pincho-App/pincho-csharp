namespace Pincho.Exceptions;

/// <summary>
/// Exception thrown when authentication fails (401/403 errors).
/// </summary>
public class AuthenticationException : PinchoException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code (401 or 403).</param>
    public AuthenticationException(string message, int statusCode)
        : base(message, statusCode)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code (401 or 403).</param>
    /// <param name="innerException">The inner exception.</param>
    public AuthenticationException(string message, int statusCode, Exception innerException)
        : base(message, statusCode, innerException)
    {
    }
}
