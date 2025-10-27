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
    private const string TestToken = "wpt_test_token";
    private const string TestUserId = "test_user_123";

    [Fact]
    public void Constructor_WithValidParameters_CreatesClient()
    {
        var client = new WirePusherClient(TestToken, TestUserId);
        Assert.NotNull(client);
    }

    [Fact]
    public void Constructor_WithTimeout_CreatesClient()
    {
        var client = new WirePusherClient(TestToken, TestUserId, TimeSpan.FromSeconds(60));
        Assert.NotNull(client);
    }

    [Theory]
    [InlineData(null, TestUserId)]
    [InlineData("", TestUserId)]
    [InlineData("   ", TestUserId)]
    public void Constructor_WithInvalidToken_ThrowsArgumentException(string? token, string userId)
    {
        Assert.Throws<ArgumentException>(() => new WirePusherClient(token!, userId));
    }

    [Theory]
    [InlineData(TestToken, null)]
    [InlineData(TestToken, "")]
    [InlineData(TestToken, "   ")]
    public void Constructor_WithInvalidUserId_ThrowsArgumentException(string token, string? userId)
    {
        Assert.Throws<ArgumentException>(() => new WirePusherClient(token, userId!));
    }

    [Fact]
    public async Task SendAsync_WithValidParameters_ReturnsSuccess()
    {
        var httpClient = CreateMockHttpClient(HttpStatusCode.OK,
            new NotificationResponse("success", "Notification sent successfully"));

        var client = new WirePusherClient(TestToken, TestUserId, httpClient);

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

        var client = new WirePusherClient(TestToken, TestUserId, httpClient);

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
        var client = new WirePusherClient(TestToken, TestUserId);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            client.SendNotificationAsync(null!));
    }

    [Fact]
    public async Task SendAsync_WithAuthenticationError_ThrowsAuthenticationException()
    {
        var httpClient = CreateMockHttpClient(HttpStatusCode.Unauthorized,
            new { status = "error", message = "Invalid token" });

        var client = new WirePusherClient(TestToken, TestUserId, httpClient);

        var exception = await Assert.ThrowsAsync<AuthenticationException>(() =>
            client.SendAsync("Test", "Message"));

        Assert.Equal(401, exception.StatusCode);
        Assert.Contains("Invalid token", exception.Message);
    }

    [Fact]
    public async Task SendAsync_WithValidationError_ThrowsValidationException()
    {
        var httpClient = CreateMockHttpClient(HttpStatusCode.BadRequest,
            new { status = "error", message = "Title is required" });

        var client = new WirePusherClient(TestToken, TestUserId, httpClient);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            client.SendAsync("", "Message"));

        Assert.Equal(400, exception.StatusCode);
        Assert.Contains("Title is required", exception.Message);
    }

    [Fact]
    public async Task SendAsync_WithRateLimitError_ThrowsRateLimitException()
    {
        var httpClient = CreateMockHttpClient((HttpStatusCode)429,
            new { status = "error", message = "Rate limit exceeded" });

        var client = new WirePusherClient(TestToken, TestUserId, httpClient);

        var exception = await Assert.ThrowsAsync<RateLimitException>(() =>
            client.SendAsync("Test", "Message"));

        Assert.Equal(429, exception.StatusCode);
        Assert.Contains("Rate limit exceeded", exception.Message);
    }

    [Fact]
    public async Task SendAsync_WithServerError_ThrowsWirePusherException()
    {
        var httpClient = CreateMockHttpClient(HttpStatusCode.InternalServerError,
            new { status = "error", message = "Server error" });

        var client = new WirePusherClient(TestToken, TestUserId, httpClient);

        var exception = await Assert.ThrowsAsync<WirePusherException>(() =>
            client.SendAsync("Test", "Message"));

        Assert.Equal(500, exception.StatusCode);
        Assert.Contains("Server error", exception.Message);
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

        var client = new WirePusherClient(TestToken, TestUserId, httpClient);

        await Assert.ThrowsAsync<WirePusherException>(() =>
            client.SendAsync("Test", "Message"));
    }

    private static HttpClient CreateMockHttpClient(HttpStatusCode statusCode, object responseContent)
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
