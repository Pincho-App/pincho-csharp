using Xunit;

namespace WirePusher.Tests;

public class NotificationTests
{
    [Fact]
    public void Notification_WithRequiredProperties_CreatesInstance()
    {
        var notification = new Notification
        {
            Title = "Test Title",
            Message = "Test Message"
        };

        Assert.Equal("Test Title", notification.Title);
        Assert.Equal("Test Message", notification.Message);
        Assert.Null(notification.Type);
        Assert.Null(notification.Tags);
        Assert.Null(notification.ImageUrl);
        Assert.Null(notification.ActionUrl);
    }

    [Fact]
    public void Notification_WithAllProperties_CreatesInstance()
    {
        var notification = new Notification
        {
            Title = "Deploy Complete",
            Message = "Version 1.2.3 deployed",
            Type = "deployment",
            Tags = new[] { "prod", "release" },
            ImageUrl = "https://example.com/img.png",
            ActionUrl = "https://example.com"
        };

        Assert.Equal("Deploy Complete", notification.Title);
        Assert.Equal("Version 1.2.3 deployed", notification.Message);
        Assert.Equal("deployment", notification.Type);
        Assert.NotNull(notification.Tags);
        Assert.Equal(2, notification.Tags.Length);
        Assert.Contains("prod", notification.Tags);
        Assert.Contains("release", notification.Tags);
        Assert.Equal("https://example.com/img.png", notification.ImageUrl);
        Assert.Equal("https://example.com", notification.ActionUrl);
    }

    [Fact]
    public void Notification_RecordEquality_WorksCorrectly()
    {
        var notification1 = new Notification
        {
            Title = "Test",
            Message = "Message"
        };

        var notification2 = new Notification
        {
            Title = "Test",
            Message = "Message"
        };

        Assert.Equal(notification1, notification2);
    }
}
