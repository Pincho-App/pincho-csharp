# WirePusher C# SDK

[![NuGet](https://img.shields.io/nuget/v/WirePusher.svg)](https://www.nuget.org/packages/WirePusher/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Official C# SDK for [WirePusher](https://wirepusher.dev) - Send push notifications to your mobile devices.

## Features

- **Zero External Dependencies** - Uses .NET 6+ built-in `HttpClient` and `System.Text.Json`
- **Async/Await** - Full async support with `CancellationToken`
- **Type Safety** - Nullable reference types enabled
- **Interface-Based** - `IWirePusherClient` for dependency injection
- **Modern C#** - Records, init-only properties, pattern matching
- **Well Tested** - >90% test coverage with xUnit
- **XML Documentation** - Complete IntelliSense support

## Requirements

- .NET 6.0 or higher

## Installation

### NuGet Package Manager

```powershell
Install-Package WirePusher
```

### .NET CLI

```bash
dotnet add package WirePusher
```

### PackageReference

```xml
<PackageReference Include="WirePusher" Version="1.0.0" />
```

## Quick Start

```csharp
using WirePusher;

var client = new WirePusherClient("wpt_your_token_here", "your_user_id");

// Send a simple notification
await client.SendAsync("Build Complete", "v1.2.3 deployed successfully");
```

## Usage Examples

### Simple Notification

```csharp
using WirePusher;

var client = new WirePusherClient("wpt_your_token_here", "your_user_id");

var response = await client.SendAsync("Hello", "World!");

if (response.IsSuccess)
{
    Console.WriteLine("Notification sent successfully!");
}
```

### Advanced Notification

```csharp
using WirePusher;

var client = new WirePusherClient("wpt_your_token_here", "your_user_id");

var notification = new Notification
{
    Title = "Deploy Complete",
    Message = "Version 1.2.3 deployed to production",
    Type = "deployment",
    Tags = new[] { "production", "release" },
    ImageUrl = "https://example.com/success.png",
    ActionUrl = "https://example.com/deployment/123"
};

var response = await client.SendNotificationAsync(notification);
```

### Custom Configuration

```csharp
using WirePusher;

// Custom timeout
var client = new WirePusherClient(
    "wpt_your_token_here",
    "your_user_id",
    TimeSpan.FromSeconds(60));
```

### Error Handling

```csharp
using WirePusher;
using WirePusher.Exceptions;

var client = new WirePusherClient(
    Environment.GetEnvironmentVariable("WIREPUSHER_TOKEN")!,
    Environment.GetEnvironmentVariable("WIREPUSHER_USER_ID")!);

try
{
    await client.SendAsync("Test", "Message");
}
catch (AuthenticationException ex)
{
    // 401/403 - Invalid token or user ID
    Console.WriteLine($"Authentication failed: {ex.Message}");
    Console.WriteLine($"Status code: {ex.StatusCode}");
}
catch (ValidationException ex)
{
    // 400/404 - Invalid parameters
    Console.WriteLine($"Validation error: {ex.Message}");
}
catch (RateLimitException ex)
{
    // 429 - Rate limit exceeded
    Console.WriteLine($"Rate limit exceeded: {ex.Message}");
}
catch (WirePusherException ex)
{
    // Other API errors
    Console.WriteLine($"API error: {ex.Message}");
}
```

### ASP.NET Core Dependency Injection

#### Configuration (appsettings.json)

```json
{
  "WirePusher": {
    "Token": "wpt_your_token_here",
    "UserId": "your_user_id"
  }
}
```

#### Service Registration (Program.cs)

```csharp
using WirePusher;

var builder = WebApplication.CreateBuilder(args);

// Register WirePusherClient as a singleton
builder.Services.AddSingleton<IWirePusherClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new WirePusherClient(
        config["WirePusher:Token"]!,
        config["WirePusher:UserId"]!
    );
});

var app = builder.Build();
```

#### Usage in Services

```csharp
using WirePusher;

public class NotificationService
{
    private readonly IWirePusherClient _wirePusherClient;

    public NotificationService(IWirePusherClient wirePusherClient)
    {
        _wirePusherClient = wirePusherClient;
    }

    public async Task NotifyDeploymentAsync(string version, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            Title = "Deploy Complete",
            Message = $"Version {version} deployed to production",
            Type = "deployment",
            Tags = new[] { "prod", "deployment" }
        };

        await _wirePusherClient.SendNotificationAsync(notification, cancellationToken);
    }
}
```

### Minimal API Example

```csharp
using WirePusher;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IWirePusherClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new WirePusherClient(
        config["WirePusher:Token"]!,
        config["WirePusher:UserId"]!
    );
});

var app = builder.Build();

app.MapPost("/notify", async (
    string title,
    string message,
    IWirePusherClient client) =>
{
    try
    {
        var response = await client.SendAsync(title, message);
        return Results.Ok(response);
    }
    catch (WirePusherException ex)
    {
        return Results.Problem(ex.Message, statusCode: ex.StatusCode);
    }
});

app.Run();
```

### Cancellation Token Support

```csharp
using WirePusher;

var client = new WirePusherClient("wpt_your_token_here", "your_user_id");

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

try
{
    await client.SendAsync("Test", "Message", cts.Token);
}
catch (WirePusherException ex) when (ex.InnerException is TaskCanceledException)
{
    Console.WriteLine("Request was cancelled");
}
```

## API Reference

### `WirePusherClient`

Main client class for sending notifications.

#### Constructors

```csharp
// Default settings
WirePusherClient(string token, string userId)

// Custom timeout
WirePusherClient(string token, string userId, TimeSpan timeout)

// Custom HttpClient (for testing)
WirePusherClient(string token, string userId, HttpClient httpClient)
```

**Parameters:**
- `token` (required): Your WirePusher API token (starts with `wpt_`)
- `userId` (required): Your WirePusher user ID
- `timeout` (optional): Request timeout (default: 30 seconds)
- `httpClient` (optional): Custom HTTP client

#### Methods

```csharp
// Simple send
Task<NotificationResponse> SendAsync(
    string title,
    string message,
    CancellationToken cancellationToken = default)

// Advanced send
Task<NotificationResponse> SendNotificationAsync(
    Notification notification,
    CancellationToken cancellationToken = default)
```

### `Notification` (Record)

Represents a notification to be sent.

**Properties:**
- `Title` (required): Notification title (max 256 characters)
- `Message` (required): Notification message (max 4096 characters)
- `Type` (optional): Notification type for categorization
- `Tags` (optional): Array of tags for filtering (max 10)
- `ImageUrl` (optional): URL to an image to display
- `ActionUrl` (optional): URL to open when notification is tapped

### `NotificationResponse` (Record)

Response from sending a notification.

**Properties:**
- `Status`: Response status (e.g., "success")
- `Message`: Response message
- `IsSuccess`: Returns true if status is "success"

### Exception Types

- `WirePusherException` - Base exception for all SDK errors
- `AuthenticationException` - Authentication failures (401, 403)
- `ValidationException` - Invalid parameters (400, 404)
- `RateLimitException` - Rate limit exceeded (429)

All exceptions include:
- `Message` - Error message
- `StatusCode` - HTTP status code (or 0 if not applicable)
- `InnerException` - Original cause if available

## Best Practices

### Environment Variables

Store credentials in configuration:

```bash
# appsettings.json
{
  "WirePusher": {
    "Token": "wpt_your_token_here",
    "UserId": "your_user_id"
  }
}
```

### Singleton Pattern

Reuse client instances across your application using DI:

```csharp
builder.Services.AddSingleton<IWirePusherClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new WirePusherClient(
        config["WirePusher:Token"]!,
        config["WirePusher:UserId"]!
    );
});
```

### Graceful Degradation

Don't let notification failures break your application:

```csharp
public async Task SendNotificationAsync(string title, string message)
{
    try
    {
        await _wirePusherClient.SendAsync(title, message);
        _logger.LogInformation("Notification sent successfully");
    }
    catch (WirePusherException ex)
    {
        // Log but don't throw - notifications are non-critical
        _logger.LogError(ex, "Failed to send notification");
    }
}
```

### Thread Safety

The `WirePusherClient` is thread-safe. You can safely share a single instance across multiple threads.

## Development

### Building

```bash
dotnet build
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# View coverage report (requires reportgenerator)
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport"
```

### Project Structure

```
csharp-sdk/
├── src/WirePusher/
│   ├── WirePusher.csproj
│   ├── WirePusherClient.cs
│   ├── IWirePusherClient.cs
│   ├── Notification.cs
│   ├── NotificationResponse.cs
│   └── Exceptions/
├── tests/WirePusher.Tests/
│   ├── WirePusher.Tests.csproj
│   ├── WirePusherClientTests.cs
│   ├── NotificationTests.cs
│   └── ExceptionTests.cs
└── README.md
```

## Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history.

## Security

For security issues, please see [SECURITY.md](SECURITY.md).

## License

MIT License - see [LICENSE](LICENSE) for details.

## Links

- [WirePusher Website](https://wirepusher.dev)
- [API Documentation](https://wirepusher.dev/api)
- [Issue Tracker](https://gitlab.com/wirepusher/csharp-sdk/-/issues)
- [GitLab Repository](https://gitlab.com/wirepusher/csharp-sdk)
- [NuGet Package](https://www.nuget.org/packages/WirePusher/)

## Support

- Email: support@wirepusher.dev
- Issues: https://gitlab.com/wirepusher/csharp-sdk/-/issues
