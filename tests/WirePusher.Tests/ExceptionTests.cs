using WirePusher.Exceptions;
using Xunit;

namespace WirePusher.Tests;

public class ExceptionTests
{
    [Fact]
    public void WirePusherException_WithMessage_CreatesException()
    {
        var exception = new WirePusherException("Test error");

        Assert.Equal("Test error", exception.Message);
        Assert.Equal(0, exception.StatusCode);
    }

    [Fact]
    public void WirePusherException_WithMessageAndStatusCode_CreatesException()
    {
        var exception = new WirePusherException("Test error", 500);

        Assert.Equal("Test error", exception.Message);
        Assert.Equal(500, exception.StatusCode);
    }

    [Fact]
    public void WirePusherException_WithInnerException_CreatesException()
    {
        var inner = new Exception("Inner");
        var exception = new WirePusherException("Test error", inner);

        Assert.Equal("Test error", exception.Message);
        Assert.Same(inner, exception.InnerException);
        Assert.Equal(0, exception.StatusCode);
    }

    [Fact]
    public void AuthenticationException_InheritsFromWirePusherException()
    {
        var exception = new AuthenticationException("Invalid token", 401);

        Assert.IsAssignableFrom<WirePusherException>(exception);
        Assert.Equal("Invalid token", exception.Message);
        Assert.Equal(401, exception.StatusCode);
    }

    [Fact]
    public void ValidationException_InheritsFromWirePusherException()
    {
        var exception = new ValidationException("Title is required", 400);

        Assert.IsAssignableFrom<WirePusherException>(exception);
        Assert.Equal("Title is required", exception.Message);
        Assert.Equal(400, exception.StatusCode);
    }

    [Fact]
    public void RateLimitException_InheritsFromWirePusherException()
    {
        var exception = new RateLimitException("Rate limit exceeded", 429);

        Assert.IsAssignableFrom<WirePusherException>(exception);
        Assert.Equal("Rate limit exceeded", exception.Message);
        Assert.Equal(429, exception.StatusCode);
    }
}
