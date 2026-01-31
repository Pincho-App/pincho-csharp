using System.Text.Json.Serialization;

namespace Pincho;

/// <summary>
/// Represents an error response from the Pincho API.
/// </summary>
/// <remarks>
/// The API returns errors in a nested format with detailed error information.
/// </remarks>
internal record ErrorResponse
{
    /// <summary>
    /// Gets or initializes the response status (always "error" for error responses).
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; init; } = "error";

    /// <summary>
    /// Gets or initializes the detailed error information.
    /// </summary>
    [JsonPropertyName("error")]
    public ErrorDetail Error { get; init; } = new();
}

/// <summary>
/// Represents detailed error information from the Pincho API.
/// </summary>
internal record ErrorDetail
{
    /// <summary>
    /// Gets or initializes the error type (e.g., "validation_error", "authentication_error").
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; init; } = "";

    /// <summary>
    /// Gets or initializes the error code (e.g., "invalid_parameter", "missing_token").
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; init; } = "";

    /// <summary>
    /// Gets or initializes the human-readable error message.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; init; } = "";

    /// <summary>
    /// Gets or initializes the parameter name that caused the error (optional).
    /// </summary>
    [JsonPropertyName("param")]
    public string? Param { get; init; }
}
