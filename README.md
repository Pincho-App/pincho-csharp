# Pincho C# Library

Official .NET client for [Pincho](https://pincho.app) push notifications.

[![NuGet](https://img.shields.io/nuget/v/Pincho.svg)](https://www.nuget.org/packages/Pincho/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## Installation

```bash
dotnet add package Pincho
```

## Quick Start

```csharp
using Pincho;

var client = new PinchoClient("YOUR_TOKEN");
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
var client = new PinchoClient("abc12345");

// Custom timeout
var client = new PinchoClient("abc12345", TimeSpan.FromSeconds(60));

// Custom retry attempts
var httpClient = new HttpClient { BaseAddress = new Uri("https://api.pincho.app/") };
httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer abc12345");
var client = new PinchoClient("abc12345", httpClient, 5);
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
builder.Services.AddSingleton<IPinchoClient>(sp =>
    new PinchoClient(builder.Configuration["Pincho:Token"]!));

// Service
public class NotificationService(IPinchoClient client)
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
- **Documentation**: https://pincho.app/help
- **Repository**: https://gitlab.com/pincho-app/pincho-csharp
- **NuGet**: https://www.nuget.org/packages/Pincho/
- **Advanced Docs**: [docs/ADVANCED.md](docs/ADVANCED.md)

## License

MIT
