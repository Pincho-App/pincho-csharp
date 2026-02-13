using System.Net.Http.Json;
using System.Text.Json;
using Pincho.Crypto;
using Pincho.Exceptions;
using Pincho.Validation;

namespace Pincho;

/// <summary>
/// Pincho API client for sending push notifications.
/// </summary>
/// <remarks>
/// This client uses .NET's built-in <see cref="HttpClient"/> for HTTP requests
/// and <see cref="System.Text.Json"/> for JSON serialization.
/// </remarks>
/// <example>
/// <code>
/// // Simple send
/// var client = new PinchoClient("abc12345", null);
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
public class PinchoClient : IPinchoClient
{
    private const string DefaultBaseUrl = "https://api.pincho.app/";
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
    private const int DefaultMaxRetries = 3;
    private static readonly TimeSpan InitialRetryDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(30);
    private const string SdkVersion = "1.0.0-alpha.1";
    private const string UserAgent = $"pincho-csharp/{SdkVersion}";

    private readonly HttpClient _httpClient;
    private readonly string _token;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly int _maxRetries;
    private TimeSpan? _lastRetryAfter;

    /// <summary>
    /// Initializes a new instance of the <see cref="PinchoClient"/> class.
    /// </summary>
    /// <param name="token">The Pincho API token.</param>
    /// <exception cref="ArgumentException">Thrown when token is null or empty.</exception>
    public PinchoClient(string token)
        : this(token, CreateDefaultHttpClient(token), DefaultMaxRetries)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PinchoClient"/> class.
    /// </summary>
    /// <param name="token">The Pincho API token.</param>
    /// <param name="timeout">The request timeout.</param>
    /// <exception cref="ArgumentException">Thrown when token is null or empty.</exception>
    public PinchoClient(string token, TimeSpan timeout)
        : this(token, CreateDefaultHttpClient(token, timeout), DefaultMaxRetries)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PinchoClient"/> class.
    /// </summary>
    /// <param name="token">The Pincho API token.</param>
    /// <param name="httpClient">A custom HTTP client (for testing or advanced scenarios).</param>
    /// <exception cref="ArgumentException">Thrown when token is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when httpClient is null.</exception>
    public PinchoClient(string token, HttpClient httpClient)
        : this(token, httpClient, DefaultMaxRetries)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PinchoClient"/> class.
    /// </summary>
    /// <param name="token">The Pincho API token.</param>
    /// <param name="httpClient">A custom HTTP client (for testing or advanced scenarios).</param>
    /// <param name="maxRetries">The maximum number of retry attempts (default: 3).</param>
    /// <exception cref="ArgumentException">Thrown when token is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when httpClient is null.</exception>
    public PinchoClient(string token, HttpClient httpClient, int maxRetries)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token is required", nameof(token));

        _token = token;
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
            // Only message is encrypted (title, type, tags, URLs remain visible)
            var finalMessage = notification.Message;
            string? ivHex = null;

            if (!string.IsNullOrWhiteSpace(notification.EncryptionPassword))
            {
                var ivResult = EncryptionUtil.GenerateIV();
                ivHex = ivResult.IVHex;

                finalMessage = EncryptionUtil.EncryptMessage(
                    notification.Message,
                    notification.EncryptionPassword,
                    ivResult.IVBytes
                );
            }

            // Normalize tags
            var normalizedTags = TagNormalizer.NormalizeTags(notification.Tags);

            // Build request payload
            var payload = new Dictionary<string, object?>
            {
                ["title"] = notification.Title,
                ["message"] = finalMessage
            };

            if (notification.Type != null)
                payload["type"] = notification.Type;

            if (normalizedTags is { Length: > 0 })
                payload["tags"] = normalizedTags;

            if (notification.ImageUrl != null)
                payload["imageURL"] = notification.ImageUrl;

            if (notification.ActionUrl != null)
                payload["actionURL"] = notification.ActionUrl;

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
        string text,
        CancellationToken cancellationToken = default)
    {
        var request = new NotifAIRequest
        {
            Text = text
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
            var finalText = request.Text;
            string? ivHex = null;

            if (!string.IsNullOrWhiteSpace(request.EncryptionPassword))
            {
                var ivResult = EncryptionUtil.GenerateIV();
                finalText = EncryptionUtil.EncryptMessage(
                    request.Text,
                    request.EncryptionPassword,
                    ivResult.IVBytes
                );
                ivHex = ivResult.IVHex;
            }

            // Build request payload
            var payload = new Dictionary<string, object?>
            {
                ["text"] = finalText
            };

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

        // Parse Retry-After header if present (for rate limiting)
        _lastRetryAfter = null;
        if (response.Headers.TryGetValues("Retry-After", out var retryAfterValues))
        {
            var retryAfterValue = retryAfterValues.FirstOrDefault();
            if (!string.IsNullOrEmpty(retryAfterValue))
            {
                // Try to parse as seconds (e.g., "120")
                if (int.TryParse(retryAfterValue, out var seconds))
                {
                    _lastRetryAfter = TimeSpan.FromSeconds(seconds);
                }
                // Try to parse as HTTP-date (e.g., "Wed, 21 Oct 2015 07:28:00 GMT")
                else if (DateTimeOffset.TryParse(retryAfterValue, out var retryDate))
                {
                    var delay = retryDate - DateTimeOffset.UtcNow;
                    if (delay > TimeSpan.Zero)
                    {
                        _lastRetryAfter = delay;
                    }
                }
            }
        }

        if (response.IsSuccessStatusCode)
        {
            try
            {
                var result = await response.Content.ReadFromJsonAsync<NotificationResponse>(
                    _jsonOptions,
                    cancellationToken);

                return result ?? throw new PinchoException("Invalid response from API");
            }
            catch (JsonException ex)
            {
                throw new PinchoException("Invalid response from API", ex);
            }
        }

        // Extract error message from nested error object
        string errorMessage;
        try
        {
            var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);
            if (errorResponse?.Error?.Message != null)
            {
                // Build descriptive error message with code and param when available
                errorMessage = errorResponse.Error.Message;

                if (!string.IsNullOrWhiteSpace(errorResponse.Error.Param))
                {
                    errorMessage = $"{errorMessage} (parameter: {errorResponse.Error.Param})";
                }

                if (!string.IsNullOrWhiteSpace(errorResponse.Error.Code))
                {
                    errorMessage = $"{errorMessage} [{errorResponse.Error.Code}]";
                }
            }
            else
            {
                errorMessage = $"HTTP {statusCode}: {content}";
            }
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
            _ => new PinchoException(errorMessage, statusCode)
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
                throw new PinchoException("Request was cancelled", ex);
            }
            catch (TaskCanceledException ex)
            {
                // Timeout errors are retryable
                if (attempt >= _maxRetries)
                    throw new NetworkException("Request timed out after " + (_maxRetries + 1) + " attempts", ex);

                attempt++;
                await DelayWithBackoffAsync(attempt, isRateLimit: false, cancellationToken);
            }
            catch (PinchoException ex) when (ex.IsRetryable && attempt < _maxRetries)
            {
                // Retry for retryable exceptions
                attempt++;
                var isRateLimit = ex is RateLimitException;
                await DelayWithBackoffAsync(attempt, isRateLimit, _lastRetryAfter, cancellationToken);
            }
            catch (PinchoException)
            {
                // Non-retryable or max retries exceeded
                throw;
            }
        }
    }

    private static async Task DelayWithBackoffAsync(int attempt, bool isRateLimit, CancellationToken cancellationToken)
    {
        await DelayWithBackoffAsync(attempt, isRateLimit, null, cancellationToken);
    }

    private static async Task DelayWithBackoffAsync(int attempt, bool isRateLimit, TimeSpan? retryAfter, CancellationToken cancellationToken)
    {
        TimeSpan delay;

        // Use Retry-After header if available (for rate limits)
        if (isRateLimit && retryAfter.HasValue && retryAfter.Value > TimeSpan.Zero)
        {
            delay = retryAfter.Value;
            // Cap at MaxRetryDelay to prevent extremely long waits
            if (delay > MaxRetryDelay)
                delay = MaxRetryDelay;
        }
        else
        {
            // Calculate exponential backoff: 1s, 2s, 4s, 8s, etc., capped at 30s
            var delaySeconds = Math.Pow(2, attempt - 1);

            // For rate limits, start with a longer initial delay (5s)
            if (isRateLimit && attempt == 1)
                delaySeconds = 5;

            delay = TimeSpan.FromSeconds(Math.Min(delaySeconds, MaxRetryDelay.TotalSeconds));
        }

        await Task.Delay(delay, cancellationToken);
    }

    private static HttpClient CreateDefaultHttpClient(string token, TimeSpan? timeout = null)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(DefaultBaseUrl),
            Timeout = timeout ?? DefaultTimeout
        };

        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        return client;
    }
}
