using WirePusher;

var token = Environment.GetEnvironmentVariable("WIREPUSHER_TOKEN") ?? "your_token_here";
var client = new WirePusherClient(token);

// Send notification with all parameters
var notification = new Notification
{
    Title = "Deploy Complete",
    Message = "Version 1.2.3 deployed to production",
    Type = "deployment",
    Tags = new[] { "production", "backend", "release" },
    ImageUrl = "https://picsum.photos/400/300",
    ActionUrl = "https://example.com/deploy/123"
};

Console.WriteLine("Sending advanced notification...");
Console.WriteLine($"Title: {notification.Title}");
Console.WriteLine($"Message: {notification.Message}");
Console.WriteLine($"Type: {notification.Type}");
Console.WriteLine($"Tags: {string.Join(", ", notification.Tags)}");
Console.WriteLine($"ImageUrl: {notification.ImageUrl}");
Console.WriteLine($"ActionUrl: {notification.ActionUrl}");

var response = await client.SendNotificationAsync(notification);
Console.WriteLine($"\nStatus: {response.Status}");
Console.WriteLine($"Message: {response.Message}");
