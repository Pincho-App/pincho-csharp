using System.Text.Json.Serialization;

namespace WirePusher;

/// <summary>
/// Response from sending a notification.
/// </summary>
/// <param name="Status">The response status (e.g., "success").</param>
/// <param name="Message">The response message.</param>
public record NotificationResponse(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("message")] string Message)
{
    /// <summary>
    /// Gets a value indicating whether the response indicates success.
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess => Status == "success";

    /// <summary>
    /// Gets or sets the AI-generated notification details (only present for NotifAI responses).
    /// </summary>
    [JsonPropertyName("notification")]
    public NotifAINotification? Notification { get; init; }
}

/// <summary>
/// Represents an AI-generated notification from the NotifAI endpoint.
/// </summary>
public record NotifAINotification
{
    /// <summary>
    /// Gets or initializes the AI-generated title.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; init; } = "";

    /// <summary>
    /// Gets or initializes the AI-generated message.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; init; } = "";

    /// <summary>
    /// Gets or initializes the AI-inferred notification type.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; init; } = "";
}
