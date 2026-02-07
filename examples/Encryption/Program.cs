using Pincho;

var token = Environment.GetEnvironmentVariable("WIREPUSHER_TOKEN") ?? "your_token_here";
var client = new PinchoClient(token);

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
Console.WriteLine("\nEncrypted fields: title, message, imageUrl, actionUrl");
Console.WriteLine("NOT encrypted: type, tags (needed for filtering/routing)");

var response = await client.SendNotificationAsync(notification);
Console.WriteLine($"\nStatus: {response.Status}");
Console.WriteLine($"Message: {response.Message}");

Console.WriteLine("\nIMPORTANT:");
Console.WriteLine("- The password must match the type configuration in your WirePusher app");
Console.WriteLine("- Encryption uses AES-128-CBC for compatibility with mobile app");
Console.WriteLine("- Encrypted: title, message, imageUrl, actionUrl (all use same IV)");
Console.WriteLine("- NOT encrypted: type, tags");
