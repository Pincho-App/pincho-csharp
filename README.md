# WirePusher C# Library

Official .NET client for [WirePusher](https://wirepusher.dev) push notifications.

[![NuGet](https://img.shields.io/nuget/v/WirePusher.svg)](https://www.nuget.org/packages/WirePusher/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## Installation

```bash
dotnet add package WirePusher
```

## Quick Start

```csharp
using WirePusher;

var client = new WirePusherClient("YOUR_TOKEN", null);
await client.SendAsync("Deploy Complete", "Version 1.2.3 deployed");

// With full options
var notification = new Notification
{
    Title = "Deploy Complete",
    Message = "Version 1.2.3 deployed",
    Type = "deployment",
    Tags = new[] { "production", "backend" }
};
await client.SendNotificationAsync(notification);
```

## Features

```csharp
// All parameters
var notification = new Notification
{
    Title = "Deploy Complete",
    Message = "Version 1.2.3 deployed",
    Type = "deployment",
    Tags = new[] { "production", "backend" },
    ImageUrl = "https://example.com/success.png",
    ActionUrl = "https://example.com/deploy/123"
};
await client.SendNotificationAsync(notification);

// AI-powered notifications (NotifAI)
var response = await client.NotifAIAsync("deployment finished, v2.1.3 is live");
// response contains AI-generated notification structure

// Encrypted messages
var encrypted = new Notification
{
    Title = "Security Alert",
    Message = "Sensitive data",
    Type = "security",
    EncryptionPassword = "your_password"
};
await client.SendNotificationAsync(encrypted);
```

## Configuration

```csharp
// Default configuration
var client = new WirePusherClient("abc12345", null);

// Custom timeout
var client = new WirePusherClient("abc12345", null, TimeSpan.FromSeconds(60));

// Custom retry attempts
var httpClient = new HttpClient { BaseAddress = new Uri("https://...") };
var client = new WirePusherClient("abc12345", null, httpClient, maxRetries: 5);
```

## Error Handling

Use typed exceptions for error handling:

```csharp
try
{
    await client.SendAsync("Title", "Message");
}
catch (AuthenticationException ex)
{
    // Invalid token (401/403) - not retried
    Console.WriteLine($"Auth failed: {ex.Message}");
}
catch (RateLimitException ex)
{
    // Rate limited (429) - automatically retried with backoff
    Console.WriteLine($"Rate limited: {ex.Message}");
}
catch (ValidationException ex)
{
    // Invalid parameters (400) - not retried
    Console.WriteLine($"Validation error: {ex.Message}");
}
catch (ServerException ex)
{
    // Server error (5xx) - automatically retried
    Console.WriteLine($"Server error: {ex.Message}");
}
catch (NetworkException ex)
{
    // Network/timeout error - automatically retried
    Console.WriteLine($"Network error: {ex.Message}");
}
```

Automatic retry with exponential backoff for network errors, 5xx, and 429 (rate limit). Respects `Retry-After` headers.

## ASP.NET Core Integration

```csharp
// Program.cs
builder.Services.AddSingleton<IWirePusherClient>(sp =>
    new WirePusherClient(builder.Configuration["WirePusher:Token"]!, null));

// Service
public class NotificationService(IWirePusherClient client)
{
    public async Task NotifyAsync(string message) =>
        await client.SendAsync("Alert", message);
}
```

## Requirements

- .NET 6.0+
- Zero external dependencies (System.Net.Http, System.Text.Json)
- Full async/await with CancellationToken support
- Nullable reference types enabled

## Links

- **Get Token**: App → Settings → Help → copy token
- **Documentation**: https://wirepusher.dev/help
- **Repository**: https://gitlab.com/wirepusher/wirepusher-csharp
- **NuGet**: https://www.nuget.org/packages/WirePusher/
- **Advanced Docs**: [docs/ADVANCED.md](docs/ADVANCED.md)

## License

MIT
