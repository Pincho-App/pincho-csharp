using WirePusher;

var token = Environment.GetEnvironmentVariable("WIREPUSHER_TOKEN") ?? "your_token_here";
var client = new WirePusherClient(token);

// IMPORTANT: Before using encryption:
// 1. In WirePusher app, create/edit a notification type
// 2. Enable encryption for that type
// 3. Set the same password in the app type configuration
// 4. Use that password here

var notification = new Notification
{
    Title = "Security Alert",
    Message = "Sensitive data: API key rotated successfully",
    Type = "security",
    EncryptionPassword = "your_encryption_password"  // Must match app config
};

Console.WriteLine("Sending encrypted notification...");
Console.WriteLine($"Title: {notification.Title}");
Console.WriteLine($"Message: {notification.Message}");
Console.WriteLine($"Type: {notification.Type}");
Console.WriteLine($"Encryption: Enabled (AES-128-CBC)");
Console.WriteLine("\nNote: Only the message is encrypted.");
Console.WriteLine("Title, type, tags, and URLs remain visible for filtering.");

var response = await client.SendNotificationAsync(notification);
Console.WriteLine($"\nStatus: {response.Status}");
Console.WriteLine($"Message: {response.Message}");

Console.WriteLine("\nIMPORTANT:");
Console.WriteLine("- The password must match the type configuration in your WirePusher app");
Console.WriteLine("- Encryption uses AES-128-CBC for compatibility with mobile app");
Console.WriteLine("- Only the message body is encrypted, not title/type/tags/URLs");
