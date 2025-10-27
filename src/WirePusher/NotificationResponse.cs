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
}
