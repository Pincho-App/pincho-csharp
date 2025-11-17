using System.Net;
using System.Text;
using System.Text.Json;
using Moq;
using Moq.Protected;
using WirePusher.Exceptions;
using Xunit;

namespace WirePusher.Tests;

public class WirePusherClientTests
{
    private const string TestToken = "abc12345";

    [Fact]
    public void Constructor_WithValidToken_CreatesClient()
    {
        var client = new WirePusherClient(TestToken);
        Assert.NotNull(client);
    }

    [Fact]
    public void Constructor_WithTimeout_CreatesClient()
    {
        var client = new WirePusherClient(TestToken, TimeSpan.FromSeconds(60));
        Assert.NotNull(client);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyToken_ThrowsArgumentException(string? token)
    {
        Assert.Throws<ArgumentException>(() => new WirePusherClient(token!));
    }

    [Fact]
    public async Task SendAsync_WithValidParameters_ReturnsSuccess()
    {
        var httpClient = CreateMockHttpClient(HttpStatusCode.OK,
            new NotificationResponse("success", "Notification sent successfully"));

        var client = new WirePusherClient(TestToken, httpClient);

        var response = await client.SendAsync("Test Title", "Test Message");

        Assert.NotNull(response);
        Assert.True(response.IsSuccess);
        Assert.Equal("success", response.Status);
        Assert.Equal("Notification sent successfully", response.Message);
    }

    [Fact]
    public async Task SendNotificationAsync_WithFullOptions_ReturnsSuccess()
    {
        var httpClient = CreateMockHttpClient(HttpStatusCode.OK,
            new NotificationResponse("success", "Notification sent successfully"));

        var client = new WirePusherClient(TestToken, httpClient);

        var notification = new Notification
        {
            Title = "Deploy Complete",
            Message = "Version 1.2.3 deployed",
            Type = "deployment",
            Tags = new[] { "prod", "release" },
            ImageUrl = "https://example.com/img.png",
            ActionUrl = "https://example.com"
        };

        var response = await client.SendNotificationAsync(notification);

        Assert.NotNull(response);
        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task SendNotificationAsync_WithNullNotification_ThrowsArgumentNullException()
    {
        var httpClient = CreateMockHttpClient(HttpStatusCode.OK,
            new NotificationResponse("success", "Test"));
        var client = new WirePusherClient(TestToken, httpClient);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            client.SendNotificationAsync(null!));
    }

    [Fact]
    public async Task SendAsync_WithAuthenticationError_ThrowsAuthenticationException()
    {
        var httpClient = CreateMockHttpClient(HttpStatusCode.Unauthorized,
            new { status = "error", error = new { type = "authentication_error", code = "invalid_token", message = "Invalid token" } });

        var client = new WirePusherClient(TestToken, httpClient);

        var exception = await Assert.ThrowsAsync<AuthenticationException>(() =>
            client.SendAsync("Test", "Message"));

        Assert.Equal(401, exception.StatusCode);
        Assert.Contains("Invalid token", exception.Message);
    }

    [Fact]
    public async Task SendAsync_WithValidationError_ThrowsValidationException()
    {
        var httpClient = CreateMockHttpClient(HttpStatusCode.BadRequest,
            new { status = "error", error = new { type = "validation_error", code = "missing_parameter", message = "Title is required", param = "title" } });

        var client = new WirePusherClient(TestToken, httpClient);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            client.SendAsync("", "Message"));

        Assert.Equal(400, exception.StatusCode);
        Assert.Contains("Title is required", exception.Message);
    }

    [Fact]
    public async Task SendAsync_WithRateLimitError_ThrowsRateLimitException()
    {
        var httpClient = CreateMockHttpClient((HttpStatusCode)429,
            new { status = "error", error = new { type = "rate_limit_error", code = "too_many_requests", message = "Rate limit exceeded" } });

        var client = new WirePusherClient(TestToken, httpClient);

        var exception = await Assert.ThrowsAsync<RateLimitException>(() =>
            client.SendAsync("Test", "Message"));

        Assert.Equal(429, exception.StatusCode);
        Assert.Contains("Rate limit exceeded", exception.Message);
    }

    [Fact]
    public async Task SendAsync_WithServerError_ThrowsServerException()
    {
        var httpClient = CreateMockHttpClient(HttpStatusCode.InternalServerError,
            new { status = "error", error = new { type = "server_error", code = "internal_error", message = "Server error" } }, maxRetries: 0);

        var client = new WirePusherClient(TestToken, httpClient, 0);

        var exception = await Assert.ThrowsAsync<ServerException>(() =>
            client.SendAsync("Test", "Message"));

        Assert.Equal(500, exception.StatusCode);
        Assert.Contains("Server error", exception.Message);
        Assert.True(exception.IsRetryable);
    }

    [Fact]
    public async Task SendAsync_WithInvalidJson_ThrowsWirePusherException()
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("not json", Encoding.UTF8, "text/plain")
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.test.com")
        };

        var client = new WirePusherClient(TestToken, httpClient);

        await Assert.ThrowsAsync<WirePusherException>(() =>
            client.SendAsync("Test", "Message"));
    }

    [Fact]
    public async Task SendNotificationAsync_WithEncryption_EncryptsMessage()
    {
        string? capturedPayload = null;

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
                capturedPayload = request.Content?.ReadAsStringAsync().Result;
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        JsonSerializer.Serialize(new NotificationResponse("success", "Sent")),
                        Encoding.UTF8,
                        "application/json")
                };
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.test.com")
        };

        var client = new WirePusherClient(TestToken, httpClient);

        var notification = new Notification
        {
            Title = "Test Title",
            Message = "Secret Message",
            EncryptionPassword = "my-password"
        };

        var response = await client.SendNotificationAsync(notification);

        Assert.True(response.IsSuccess);
        Assert.NotNull(capturedPayload);

        // Parse the payload
        var payload = JsonSerializer.Deserialize<JsonElement>(capturedPayload);

        // Verify message is encrypted (not plaintext)
        var encryptedMessage = payload.GetProperty("message").GetString();
        Assert.NotNull(encryptedMessage);
        Assert.NotEqual("Secret Message", encryptedMessage);

        // Verify IV is present
        Assert.True(payload.TryGetProperty("iv", out var ivElement));
        var iv = ivElement.GetString();
        Assert.NotNull(iv);
        Assert.Equal(32, iv.Length); // 16 bytes as hex = 32 characters

        // Verify encrypted message uses custom Base64
        Assert.DoesNotContain('+', encryptedMessage);
        Assert.DoesNotContain('/', encryptedMessage);
        Assert.DoesNotContain('=', encryptedMessage);
    }

    [Fact]
    public async Task SendNotificationAsync_WithoutEncryption_SendsPlaintext()
    {
        string? capturedPayload = null;

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
                capturedPayload = request.Content?.ReadAsStringAsync().Result;
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        JsonSerializer.Serialize(new NotificationResponse("success", "Sent")),
                        Encoding.UTF8,
                        "application/json")
                };
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.test.com")
        };

        var client = new WirePusherClient(TestToken, httpClient);

        var notification = new Notification
        {
            Title = "Test Title",
            Message = "Plain Message"
            // No EncryptionPassword
        };

        var response = await client.SendNotificationAsync(notification);

        Assert.True(response.IsSuccess);
        Assert.NotNull(capturedPayload);

        // Parse the payload
        var payload = JsonSerializer.Deserialize<JsonElement>(capturedPayload);

        // Verify message is plaintext
        var message = payload.GetProperty("message").GetString();
        Assert.Equal("Plain Message", message);

        // Verify no IV is present
        Assert.False(payload.TryGetProperty("iv", out _));
    }

    [Fact]
    public async Task SendNotificationAsync_WithEncryption_OnlyEncryptsMessage()
    {
        string? capturedPayload = null;

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
                capturedPayload = request.Content?.ReadAsStringAsync().Result;
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        JsonSerializer.Serialize(new NotificationResponse("success", "Sent")),
                        Encoding.UTF8,
                        "application/json")
                };
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.test.com")
        };

        var client = new WirePusherClient(TestToken, httpClient);

        var notification = new Notification
        {
            Title = "Public Title",
            Message = "Secret Message",
            Type = "info",
            Tags = new[] { "tag1", "tag2" },
            ImageUrl = "https://example.com/image.png",
            ActionUrl = "https://example.com",
            EncryptionPassword = "my-password"
        };

        var response = await client.SendNotificationAsync(notification);

        Assert.True(response.IsSuccess);
        Assert.NotNull(capturedPayload);

        var payload = JsonSerializer.Deserialize<JsonElement>(capturedPayload);

        // Verify title is NOT encrypted
        Assert.Equal("Public Title", payload.GetProperty("title").GetString());

        // Verify message IS encrypted
        var encryptedMessage = payload.GetProperty("message").GetString();
        Assert.NotEqual("Secret Message", encryptedMessage);

        // Verify other fields are NOT encrypted
        Assert.Equal("info", payload.GetProperty("type").GetString());
        Assert.Equal("https://example.com/image.png", payload.GetProperty("imageURL").GetString());
        Assert.Equal("https://example.com", payload.GetProperty("actionURL").GetString());

        var tags = payload.GetProperty("tags");
        Assert.Equal(2, tags.GetArrayLength());
    }

    [Fact]
    public async Task SendNotificationAsync_WithTags_NormalizesTags()
    {
        string? capturedPayload = null;

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
                capturedPayload = request.Content?.ReadAsStringAsync().Result;
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        JsonSerializer.Serialize(new NotificationResponse("success", "Sent")),
                        Encoding.UTF8,
                        "application/json")
                };
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.test.com")
        };

        var client = new WirePusherClient(TestToken, httpClient);

        var notification = new Notification
        {
            Title = "Test",
            Message = "Message",
            Tags = new[] { "  PROD  ", "prod", "Backend@123", "test-env_1", "TEST-ENV_1" }
        };

        await client.SendNotificationAsync(notification);

        Assert.NotNull(capturedPayload);
        var payload = JsonSerializer.Deserialize<JsonElement>(capturedPayload);
        var tags = payload.GetProperty("tags");
        Assert.Equal(3, tags.GetArrayLength());
        Assert.Equal("prod", tags[0].GetString());
        Assert.Equal("backend123", tags[1].GetString());
        Assert.Equal("test-env_1", tags[2].GetString());
    }

    [Fact]
    public async Task NotifAIAsync_WithSimpleInput_SendsRequest()
    {
        var httpClient = CreateMockHttpClient(HttpStatusCode.OK,
            new NotificationResponse("success", "AI notification sent"));

        var client = new WirePusherClient(TestToken, httpClient);

        var response = await client.NotifAIAsync("deployment finished, v2.1.3 is live");

        Assert.NotNull(response);
        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task NotifAIAsync_WithRequest_SendsRequest()
    {
        string? capturedPayload = null;

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
                capturedPayload = request.Content?.ReadAsStringAsync().Result;
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        JsonSerializer.Serialize(new NotificationResponse("success", "Sent")),
                        Encoding.UTF8,
                        "application/json")
                };
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.test.com")
        };

        var client = new WirePusherClient(TestToken, httpClient);

        var request = new NotifAIRequest
        {
            Text = "server CPU at 95%",
            Type = "alert"
        };

        await client.NotifAIAsync(request);

        Assert.NotNull(capturedPayload);
        var payload = JsonSerializer.Deserialize<JsonElement>(capturedPayload);
        Assert.Equal("server CPU at 95%", payload.GetProperty("text").GetString());
        Assert.Equal("alert", payload.GetProperty("type").GetString());
    }

    [Fact]
    public async Task SendAsync_WithNetworkError_RetriesAndThrowsNetworkException()
    {
        var attemptCount = 0;
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                attemptCount++;
                throw new HttpRequestException("Network error");
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.test.com")
        };

        var client = new WirePusherClient(TestToken, httpClient,2);

        var exception = await Assert.ThrowsAsync<NetworkException>(() =>
            client.SendAsync("Test", "Message"));

        Assert.Equal(3, attemptCount); // Initial attempt + 2 retries
        Assert.Contains("after 3 attempts", exception.Message);
        Assert.True(exception.IsRetryable);
    }

    [Fact]
    public async Task SendAsync_WithRateLimit_RetriesWithBackoff()
    {
        var attemptCount = 0;
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                attemptCount++;
                if (attemptCount <= 2)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = (HttpStatusCode)429,
                        Content = new StringContent(
                            JsonSerializer.Serialize(new { status = "error", message = "Rate limit exceeded" }),
                            Encoding.UTF8,
                            "application/json")
                    };
                }
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        JsonSerializer.Serialize(new NotificationResponse("success", "Sent")),
                        Encoding.UTF8,
                        "application/json")
                };
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.test.com"),
            Timeout = TimeSpan.FromMinutes(1)
        };

        var client = new WirePusherClient(TestToken, httpClient,3);

        var response = await client.SendAsync("Test", "Message");

        Assert.Equal(3, attemptCount); // Should retry twice and succeed on third attempt
        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task SendAsync_WithServerError_RetriesAndSucceeds()
    {
        var attemptCount = 0;
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                attemptCount++;
                if (attemptCount == 1)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.InternalServerError,
                        Content = new StringContent(
                            JsonSerializer.Serialize(new { status = "error", message = "Server error" }),
                            Encoding.UTF8,
                            "application/json")
                    };
                }
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        JsonSerializer.Serialize(new NotificationResponse("success", "Sent")),
                        Encoding.UTF8,
                        "application/json")
                };
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.test.com"),
            Timeout = TimeSpan.FromMinutes(1)
        };

        var client = new WirePusherClient(TestToken, httpClient,3);

        var response = await client.SendAsync("Test", "Message");

        Assert.Equal(2, attemptCount); // Should fail once, then succeed
        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task SendAsync_WithValidationError_DoesNotRetry()
    {
        var attemptCount = 0;
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                attemptCount++;
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent(
                        JsonSerializer.Serialize(new { status = "error", message = "Title is required" }),
                        Encoding.UTF8,
                        "application/json")
                };
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.test.com")
        };

        var client = new WirePusherClient(TestToken, httpClient,3);

        await Assert.ThrowsAsync<ValidationException>(() => client.SendAsync("", "Message"));

        Assert.Equal(1, attemptCount); // Should not retry validation errors
    }

    private static HttpClient CreateMockHttpClient(HttpStatusCode statusCode, object responseContent, int maxRetries = 0)
    {
        var json = JsonSerializer.Serialize(responseContent);
        var mockHandler = new Mock<HttpMessageHandler>();

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        return new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.test.com")
        };
    }
}
