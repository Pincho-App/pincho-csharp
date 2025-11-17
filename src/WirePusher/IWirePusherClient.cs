namespace WirePusher;

/// <summary>
/// Interface for the WirePusher API client.
/// </summary>
public interface IWirePusherClient
{
    /// <summary>
    /// Sends a simple notification with title and message.
    /// </summary>
    /// <param name="title">The notification title (required, max 256 characters).</param>
    /// <param name="message">The notification message (required, max 4096 characters).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The notification response.</returns>
    /// <exception cref="WirePusher.Exceptions.WirePusherException">Thrown when the request fails.</exception>
    Task<NotificationResponse> SendAsync(
        string title,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification with full options.
    /// </summary>
    /// <param name="notification">The notification to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The notification response.</returns>
    /// <exception cref="WirePusher.Exceptions.WirePusherException">Thrown when the request fails.</exception>
    Task<NotificationResponse> SendNotificationAsync(
        Notification notification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an AI-generated notification using free-form text.
    /// </summary>
    /// <param name="text">The free-form text to convert to a notification.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The notification response.</returns>
    /// <exception cref="WirePusher.Exceptions.WirePusherException">Thrown when the request fails.</exception>
    /// <remarks>
    /// NotifAI uses Gemini AI to convert free-form text into structured notifications
    /// with automatically generated title, message, tags, and action URL.
    /// Examples: "deployment finished, v2.1.3 is live", "server CPU at 95%"
    /// </remarks>
    Task<NotificationResponse> NotifAIAsync(
        string text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an AI-generated notification with full options.
    /// </summary>
    /// <param name="request">The NotifAI request with input and optional type/encryption.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The notification response.</returns>
    /// <exception cref="WirePusher.Exceptions.WirePusherException">Thrown when the request fails.</exception>
    /// <remarks>
    /// NotifAI uses Gemini AI to convert free-form text into structured notifications
    /// with automatically generated title, message, tags, and action URL.
    /// </remarks>
    Task<NotificationResponse> NotifAIAsync(
        NotifAIRequest request,
        CancellationToken cancellationToken = default);
}
