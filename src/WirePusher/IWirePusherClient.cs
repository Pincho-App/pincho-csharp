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
}
