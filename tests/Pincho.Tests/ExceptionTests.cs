using Pincho.Exceptions;
using Xunit;

namespace Pincho.Tests;

public class ExceptionTests
{
    [Fact]
    public void PinchoException_WithMessage_CreatesException()
    {
        var exception = new PinchoException("Test error");

        Assert.Equal("Test error", exception.Message);
        Assert.Equal(0, exception.StatusCode);
    }

    [Fact]
    public void PinchoException_WithMessageAndStatusCode_CreatesException()
    {
        var exception = new PinchoException("Test error", 500);

        Assert.Equal("Test error", exception.Message);
        Assert.Equal(500, exception.StatusCode);
    }

    [Fact]
    public void PinchoException_WithInnerException_CreatesException()
    {
        var inner = new Exception("Inner");
        var exception = new PinchoException("Test error", inner);

        Assert.Equal("Test error", exception.Message);
        Assert.Same(inner, exception.InnerException);
        Assert.Equal(0, exception.StatusCode);
    }

    [Fact]
    public void AuthenticationException_InheritsFromPinchoException()
    {
        var exception = new AuthenticationException("Invalid token", 401);

        Assert.IsAssignableFrom<PinchoException>(exception);
        Assert.Equal("Invalid token", exception.Message);
        Assert.Equal(401, exception.StatusCode);
    }

    [Fact]
    public void ValidationException_InheritsFromPinchoException()
    {
        var exception = new ValidationException("Title is required", 400);

        Assert.IsAssignableFrom<PinchoException>(exception);
        Assert.Equal("Title is required", exception.Message);
        Assert.Equal(400, exception.StatusCode);
    }

    [Fact]
    public void RateLimitException_InheritsFromPinchoException()
    {
        var exception = new RateLimitException("Rate limit exceeded", 429);

        Assert.IsAssignableFrom<PinchoException>(exception);
        Assert.Equal("Rate limit exceeded", exception.Message);
        Assert.Equal(429, exception.StatusCode);
    }

    [Fact]
    public void PinchoException_IsNotRetryable_ByDefault()
    {
        var exception = new PinchoException("Test error");
        Assert.False(exception.IsRetryable);
    }

    [Fact]
    public void AuthenticationException_IsNotRetryable()
    {
        var exception = new AuthenticationException("Invalid token", 401);
        Assert.False(exception.IsRetryable);
    }

    [Fact]
    public void ValidationException_IsNotRetryable()
    {
        var exception = new ValidationException("Title is required", 400);
        Assert.False(exception.IsRetryable);
    }

    [Fact]
    public void RateLimitException_IsRetryable()
    {
        var exception = new RateLimitException("Rate limit exceeded", 429);
        Assert.True(exception.IsRetryable);
    }

    [Fact]
    public void ServerException_InheritsFromPinchoException()
    {
        var exception = new ServerException("Internal server error", 500);

        Assert.IsAssignableFrom<PinchoException>(exception);
        Assert.Equal("Internal server error", exception.Message);
        Assert.Equal(500, exception.StatusCode);
    }

    [Fact]
    public void ServerException_IsRetryable()
    {
        var exception = new ServerException("Internal server error", 500);
        Assert.True(exception.IsRetryable);
    }

    [Fact]
    public void NetworkException_InheritsFromPinchoException()
    {
        var exception = new NetworkException("Connection failed");

        Assert.IsAssignableFrom<PinchoException>(exception);
        Assert.Equal("Connection failed", exception.Message);
        Assert.Equal(0, exception.StatusCode);
    }

    [Fact]
    public void NetworkException_IsRetryable()
    {
        var exception = new NetworkException("Connection failed");
        Assert.True(exception.IsRetryable);
    }

    [Fact]
    public void NetworkException_WithInnerException_PreservesInner()
    {
        var inner = new Exception("Inner");
        var exception = new NetworkException("Connection failed", inner);

        Assert.Same(inner, exception.InnerException);
        Assert.True(exception.IsRetryable);
    }
}
