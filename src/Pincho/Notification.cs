using System.Text.Json.Serialization;

namespace Pincho;

/// <summary>
/// Represents a notification to be sent via Pincho.
/// </summary>
public record Notification
{
    /// <summary>
    /// Gets or initializes the notification title (required, max 256 characters).
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the notification message (required, max 4096 characters).
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the notification type for categorization (optional).
    /// </summary>
    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; init; }

    /// <summary>
    /// Gets or initializes the tags for filtering (optional, max 10).
    /// </summary>
    [JsonPropertyName("tags")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Tags { get; init; }

    /// <summary>
    /// Gets or initializes the URL to an image to display (optional).
    /// </summary>
    [JsonPropertyName("imageURL")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ImageUrl { get; init; }

    /// <summary>
    /// Gets or initializes the URL to open when notification is tapped (optional).
    /// </summary>
    [JsonPropertyName("actionURL")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ActionUrl { get; init; }

    /// <summary>
    /// Gets or initializes the encryption password for AES-128-CBC encryption (optional).
    /// </summary>
    /// <remarks>
    /// When provided, the message will be encrypted client-side before sending.
    /// Password must match the type configuration in the Pincho app.
    /// Password is never sent to the API - used only for local encryption.
    /// </remarks>
    [JsonIgnore]
    public string? EncryptionPassword { get; init; }
}
