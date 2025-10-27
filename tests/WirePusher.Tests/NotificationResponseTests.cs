using Xunit;

namespace WirePusher.Tests;

public class NotificationResponseTests
{
    [Fact]
    public void NotificationResponse_WithSuccessStatus_IsSuccess()
    {
        var response = new NotificationResponse("success", "Notification sent successfully");

        Assert.Equal("success", response.Status);
        Assert.Equal("Notification sent successfully", response.Message);
        Assert.True(response.IsSuccess);
    }

    [Fact]
    public void NotificationResponse_WithErrorStatus_IsNotSuccess()
    {
        var response = new NotificationResponse("error", "Failed");

        Assert.Equal("error", response.Status);
        Assert.Equal("Failed", response.Message);
        Assert.False(response.IsSuccess);
    }

    [Fact]
    public void NotificationResponse_RecordEquality_WorksCorrectly()
    {
        var response1 = new NotificationResponse("success", "OK");
        var response2 = new NotificationResponse("success", "OK");

        Assert.Equal(response1, response2);
    }
}
