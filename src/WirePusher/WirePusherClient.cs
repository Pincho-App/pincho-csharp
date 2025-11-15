using System.Net.Http.Json;
using System.Text.Json;
using WirePusher.Crypto;
using WirePusher.Exceptions;
using WirePusher.Validation;

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
    private const int DefaultMaxRetries = 3;
    private static readonly TimeSpan InitialRetryDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(30);

    private readonly HttpClient _httpClient;
    private readonly string? _token;
    private readonly string? _deviceId;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly int _maxRetries;

    /// <summary>
    /// Initializes a new instance of the <see cref="WirePusherClient"/> class.
    /// </summary>
    /// <param name="token">The WirePusher API token (pass null if using deviceId).</param>
    /// <param name="deviceId">The WirePusher device ID (pass null if using token). DEPRECATED: Legacy authentication. Use Token parameter instead.</param>
    /// <exception cref="ArgumentException">Thrown when both or neither credentials are provided.</exception>
    public WirePusherClient(
        string? token,
        string? deviceId)
        : this(token, deviceId, CreateDefaultHttpClient(), DefaultMaxRetries)
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
        : this(token, deviceId, CreateDefaultHttpClient(timeout), DefaultMaxRetries)
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
        : this(token, deviceId, httpClient, DefaultMaxRetries)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WirePusherClient"/> class.
    /// </summary>
    /// <param name="token">The WirePusher API token (pass null if using deviceId).</param>
    /// <param name="deviceId">The WirePusher device ID (pass null if using token). DEPRECATED: Legacy authentication. Use Token parameter instead.</param>
    /// <param name="httpClient">A custom HTTP client (for testing or advanced scenarios).</param>
    /// <param name="maxRetries">The maximum number of retry attempts (default: 3).</param>
    /// <exception cref="ArgumentException">Thrown when both or neither credentials are provided.</exception>
    /// <exception cref="ArgumentNullException">Thrown when httpClient is null.</exception>
    public WirePusherClient(
        string? token,
        string? deviceId,
        HttpClient httpClient,
        int maxRetries)
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
        _maxRetries = maxRetries >= 0 ? maxRetries : DefaultMaxRetries;

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

        return await ExecuteWithRetryAsync(async () =>
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

            // Normalize tags
            var normalizedTags = TagNormalizer.NormalizeTags(notification.Tags);

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

            if (normalizedTags is { Length: > 0 })
                payload["tags"] = normalizedTags;

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
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<NotificationResponse> NotifAIAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        var request = new NotifAIRequest
        {
            Input = input
        };

        return await NotifAIAsync(request, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<NotificationResponse> NotifAIAsync(
        NotifAIRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await ExecuteWithRetryAsync(async () =>
        {
            // Handle encryption if password provided
            var finalInput = request.Input;
            string? ivHex = null;

            if (!string.IsNullOrWhiteSpace(request.EncryptionPassword))
            {
                var ivResult = EncryptionUtil.GenerateIV();
                finalInput = EncryptionUtil.EncryptMessage(
                    request.Input,
                    request.EncryptionPassword,
                    ivResult.IVBytes
                );
                ivHex = ivResult.IVHex;
            }

            // Build request payload
            var payload = new Dictionary<string, object?>
            {
                ["input"] = finalInput
            };

            // Add authentication (token XOR deviceId)
            if (_token != null)
                payload["token"] = _token;

            if (_deviceId != null)
                payload["id"] = _deviceId;

            if (request.Type != null)
                payload["type"] = request.Type;

            if (ivHex != null)
                payload["iv"] = ivHex;

            // Send HTTP request
            var response = await _httpClient.PostAsJsonAsync(
                "notifai",
                payload,
                _jsonOptions,
                cancellationToken);

            return await HandleResponseAsync(response, cancellationToken);
        }, cancellationToken);
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
            >= 500 and < 600 => new ServerException(errorMessage, statusCode),
            _ => new WirePusherException(errorMessage, statusCode)
        };
    }

    private async Task<NotificationResponse> ExecuteWithRetryAsync(
        Func<Task<NotificationResponse>> operation,
        CancellationToken cancellationToken)
    {
        var attempt = 0;
        while (true)
        {
            try
            {
                return await operation();
            }
            catch (HttpRequestException ex)
            {
                // Network errors are retryable
                if (attempt >= _maxRetries)
                    throw new NetworkException("Failed to send notification after " + (_maxRetries + 1) + " attempts: " + ex.Message, ex);

                attempt++;
                await DelayWithBackoffAsync(attempt, isRateLimit: false, cancellationToken);
            }
            catch (TaskCanceledException ex) when (ex.CancellationToken == cancellationToken)
            {
                throw new WirePusherException("Request was cancelled", ex);
            }
            catch (TaskCanceledException ex)
            {
                // Timeout errors are retryable
                if (attempt >= _maxRetries)
                    throw new NetworkException("Request timed out after " + (_maxRetries + 1) + " attempts", ex);

                attempt++;
                await DelayWithBackoffAsync(attempt, isRateLimit: false, cancellationToken);
            }
            catch (WirePusherException ex) when (ex.IsRetryable && attempt < _maxRetries)
            {
                // Retry for retryable exceptions
                attempt++;
                var isRateLimit = ex is RateLimitException;
                await DelayWithBackoffAsync(attempt, isRateLimit, cancellationToken);
            }
            catch (WirePusherException)
            {
                // Non-retryable or max retries exceeded
                throw;
            }
        }
    }

    private static async Task DelayWithBackoffAsync(int attempt, bool isRateLimit, CancellationToken cancellationToken)
    {
        // Calculate exponential backoff: 1s, 2s, 4s, 8s, etc., capped at 30s
        var delaySeconds = Math.Pow(2, attempt - 1);

        // For rate limits, start with a longer initial delay (5s)
        if (isRateLimit && attempt == 1)
            delaySeconds = 5;

        var delay = TimeSpan.FromSeconds(Math.Min(delaySeconds, MaxRetryDelay.TotalSeconds));

        await Task.Delay(delay, cancellationToken);
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
