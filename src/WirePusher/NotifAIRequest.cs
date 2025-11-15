using System.Text.Json.Serialization;

namespace WirePusher;

/// <summary>
/// Represents a request to the NotifAI endpoint for AI-generated notifications.
/// </summary>
/// <remarks>
/// NotifAI uses Gemini AI to convert free-form text into structured notifications
/// with automatically generated title, message, tags, and action URL.
/// </remarks>
public record NotifAIRequest
{
    /// <summary>
    /// Gets or initializes the free-form input text to convert to a notification (required).
    /// </summary>
    /// <remarks>
    /// Examples: "deployment finished, v2.1.3 is live", "server CPU at 95%", "backup completed successfully"
    /// </remarks>
    [JsonPropertyName("input")]
    public string Input { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the notification type for categorization (optional).
    /// </summary>
    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; init; }

    /// <summary>
    /// Gets or initializes the encryption password for AES-128-CBC encryption (optional).
    /// </summary>
    /// <remarks>
    /// When provided, the message will be encrypted client-side before sending.
    /// Password must match the type configuration in the WirePusher app.
    /// Password is never sent to the API - used only for local encryption.
    /// </remarks>
    [JsonIgnore]
    public string? EncryptionPassword { get; init; }
}
