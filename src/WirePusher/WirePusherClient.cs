using System.Net.Http.Json;
using System.Text.Json;
using WirePusher.Crypto;
using WirePusher.Exceptions;

namespace WirePusher;

/// <summary>
/// WirePusher API client for sending push notifications.
/// </summary>
/// <remarks>
/// This client uses .NET's built-in <see cref="HttpClient"/> for HTTP requests
/// and <see cref="System.Text.Json"/> for JSON serialization.
/// </remarks>
/// <example>
/// <code>
/// // Simple send
/// var client = new WirePusherClient("wpt_your_token", null);
/// await client.SendAsync("Build Failed", "Pipeline #123 failed");
///
/// // Advanced send
/// var notification = new Notification
/// {
///     Title = "Deploy Complete",
///     Message = "v1.2.3 deployed",
///     Type = "deployment",
///     Tags = new[] { "prod", "release" }
/// };
/// await client.SendNotificationAsync(notification);
/// </code>
/// </example>
public class WirePusherClient : IWirePusherClient
{
    private const string DefaultBaseUrl = "https://wirepusher-gateway-1xatwfdc.uc.gateway.dev";
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    private readonly HttpClient _httpClient;
    private readonly string? _token;
    private readonly string? _deviceId;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="WirePusherClient"/> class.
    /// </summary>
    /// <param name="token">The WirePusher API token (pass null if using deviceId).</param>
    /// <param name="deviceId">The WirePusher device ID (pass null if using token). DEPRECATED: Legacy authentication. Use Token parameter instead.</param>
    /// <exception cref="ArgumentException">Thrown when both or neither credentials are provided.</exception>
    public WirePusherClient(
        string? token,
        string? deviceId)
        : this(token, deviceId, CreateDefaultHttpClient())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WirePusherClient"/> class.
    /// </summary>
    /// <param name="token">The WirePusher API token (pass null if using deviceId).</param>
    /// <param name="deviceId">The WirePusher device ID (pass null if using token). DEPRECATED: Legacy authentication. Use Token parameter instead.</param>
    /// <param name="timeout">The request timeout.</param>
    /// <exception cref="ArgumentException">Thrown when both or neither credentials are provided.</exception>
    public WirePusherClient(
        string? token,
        string? deviceId,
        TimeSpan timeout)
        : this(token, deviceId, CreateDefaultHttpClient(timeout))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WirePusherClient"/> class.
    /// </summary>
    /// <param name="token">The WirePusher API token (pass null if using deviceId).</param>
    /// <param name="deviceId">The WirePusher device ID (pass null if using token). DEPRECATED: Legacy authentication. Use Token parameter instead.</param>
    /// <param name="httpClient">A custom HTTP client (for testing or advanced scenarios).</param>
    /// <exception cref="ArgumentException">Thrown when both or neither credentials are provided.</exception>
    /// <exception cref="ArgumentNullException">Thrown when httpClient is null.</exception>
    public WirePusherClient(
        string? token,
        string? deviceId,
        HttpClient httpClient)
    {
        var hasToken = !string.IsNullOrWhiteSpace(token);
        var hasDeviceId = !string.IsNullOrWhiteSpace(deviceId);

        if (!hasToken && !hasDeviceId)
            throw new ArgumentException("Either token or deviceId is required");

        if (hasToken && hasDeviceId)
            throw new ArgumentException("Token and deviceId are mutually exclusive - use one or the other, not both");

        _token = token;
        _deviceId = deviceId;
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <inheritdoc/>
    public async Task<NotificationResponse> SendAsync(
        string title,
        string message,
        CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            Title = title,
            Message = message
        };

        return await SendNotificationAsync(notification, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<NotificationResponse> SendNotificationAsync(
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        try
        {
            // Handle encryption if password provided
            var finalMessage = notification.Message;
            string? ivHex = null;

            if (!string.IsNullOrWhiteSpace(notification.EncryptionPassword))
            {
                var ivResult = EncryptionUtil.GenerateIV();
                finalMessage = EncryptionUtil.EncryptMessage(
                    notification.Message,
                    notification.EncryptionPassword,
                    ivResult.IVBytes
                );
                ivHex = ivResult.IVHex;
            }

            // Build request payload
            var payload = new Dictionary<string, object?>
            {
                ["title"] = notification.Title,
                ["message"] = finalMessage
            };

            // Add authentication (token XOR deviceId)
            if (_token != null)
                payload["token"] = _token;

            if (_deviceId != null)
                payload["id"] = _deviceId;

            if (notification.Type != null)
                payload["type"] = notification.Type;

            if (notification.Tags is { Length: > 0 })
                payload["tags"] = notification.Tags;

            if (notification.ImageUrl != null)
                payload["image_url"] = notification.ImageUrl;

            if (notification.ActionUrl != null)
                payload["action_url"] = notification.ActionUrl;

            if (ivHex != null)
                payload["iv"] = ivHex;

            // Send HTTP request
            var response = await _httpClient.PostAsJsonAsync(
                "send",
                payload,
                _jsonOptions,
                cancellationToken);

            return await HandleResponseAsync(response, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new WirePusherException("Failed to send notification: " + ex.Message, ex);
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken == cancellationToken)
        {
            throw new WirePusherException("Request was cancelled", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new WirePusherException("Request timed out", ex);
        }
    }

    private async Task<NotificationResponse> HandleResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var statusCode = (int)response.StatusCode;
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            try
            {
                var result = await response.Content.ReadFromJsonAsync<NotificationResponse>(
                    _jsonOptions,
                    cancellationToken);

                return result ?? throw new WirePusherException("Invalid response from API");
            }
            catch (JsonException ex)
            {
                throw new WirePusherException("Invalid response from API", ex);
            }
        }

        // Extract error message
        string errorMessage;
        try
        {
            var errorBody = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
            errorMessage = errorBody?.TryGetValue("message", out var msg) == true
                ? msg.GetString() ?? "Unknown error"
                : "Unknown error";
        }
        catch
        {
            errorMessage = $"HTTP {statusCode}: {content}";
        }

        // Throw appropriate exception
        throw statusCode switch
        {
            400 or 404 => new ValidationException(errorMessage, statusCode),
            401 or 403 => new AuthenticationException(errorMessage, statusCode),
            429 => new RateLimitException(errorMessage, statusCode),
            _ => new WirePusherException(errorMessage, statusCode)
        };
    }

    private static HttpClient CreateDefaultHttpClient(TimeSpan? timeout = null)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(DefaultBaseUrl),
            Timeout = timeout ?? DefaultTimeout
        };

        client.DefaultRequestHeaders.Add("Accept", "application/json");

        return client;
    }
}
