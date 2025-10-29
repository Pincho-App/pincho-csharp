# WirePusher C# SDK

[![NuGet](https://img.shields.io/nuget/v/WirePusher.svg)](https://www.nuget.org/packages/WirePusher/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)

Official C# SDK for [WirePusher](https://wirepusher.dev) push notifications.

## Features

- Zero external dependencies - Uses .NET 6+ built-in libraries
- Full async/await support with CancellationToken
- Type safety with nullable reference types
- Interface-based design for dependency injection
- Complete IntelliSense XML documentation
- 90%+ test coverage

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

var client = new WirePusherClient(
    Environment.GetEnvironmentVariable("WIREPUSHER_TOKEN")!);

await client.SendAsync(
    "Deploy Complete",
    "Version 1.2.3 deployed to production");
```

**Get your token:** Open app → Settings → Help → copy token

## Usage

### Basic Example

```csharp
using WirePusher;

var client = new WirePusherClient("wpt_abc123xyz");

var response = await client.SendAsync(
    "Deploy Complete",
    "Version 1.2.3 deployed to production");

Console.WriteLine(response.Status);  // "success"
```

### All Parameters

```csharp
using WirePusher;

var client = new WirePusherClient("wpt_abc123xyz");

var notification = new Notification
{
    Title = "Deploy Complete",
    Message = "Version 1.2.3 deployed to production",
    Type = "deployment",
    Tags = new[] { "production", "backend" },
    ImageUrl = "https://cdn.example.com/success.png",
    ActionUrl = "https://dash.example.com/deploy/123"
};

var response = await client.SendNotificationAsync(notification);
```

### ASP.NET Core Integration

**Configuration (appsettings.json):**

```json
{
  "WirePusher": {
    "Token": "wpt_your_token_here"
  }
}
```

**Service Registration (Program.cs):**

```csharp
using WirePusher;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IWirePusherClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new WirePusherClient(config["WirePusher:Token"]!);
});

var app = builder.Build();
```

**Usage in Services:**

```csharp
using WirePusher;

public class NotificationService
{
    private readonly IWirePusherClient _wirePusherClient;

    public NotificationService(IWirePusherClient wirePusherClient)
    {
        _wirePusherClient = wirePusherClient;
    }

    public async Task NotifyDeploymentAsync(string version)
    {
        var notification = new Notification
        {
            Title = "Deploy Complete",
            Message = $"Version {version} deployed to production",
            Type = "deployment",
            Tags = new[] { "production", "backend" }
        };

        await _wirePusherClient.SendNotificationAsync(notification);
    }
}
```

## Encryption

Encrypt notification messages using AES-128-CBC. Only the `Message` field is encrypted—`Title`, `Type`, `Tags`, `ImageUrl`, and `ActionUrl` remain unencrypted for filtering and display.

**Setup:**
1. In the app, create a notification type
2. Set an encryption password for that type
3. Pass the same `Type` and password when sending

```csharp
using WirePusher;

var client = new WirePusherClient(
    Environment.GetEnvironmentVariable("WIREPUSHER_TOKEN")!);

var notification = new Notification
{
    Title = "Security Alert",
    Message = "Unauthorized access attempt detected",
    Type = "security",
    EncryptionPassword = Environment.GetEnvironmentVariable("ENCRYPTION_PASSWORD")
};

var response = await client.SendNotificationAsync(notification);
```

**Security notes:**
- Use strong passwords (minimum 12 characters)
- Store passwords securely (environment variables, secret managers)
- Password must match the type configuration in the app

## API Reference

### WirePusherClient

**Constructors:**

```csharp
// Default settings (recommended)
WirePusherClient(string token)

// Custom timeout
WirePusherClient(string token, TimeSpan timeout)

// Custom HttpClient (for testing)
WirePusherClient(string token, HttpClient httpClient)
```

**Parameters:**
- `token` (required): Your WirePusher token (starts with `wpt_`)
- `timeout` (optional): Request timeout (default: 30 seconds)
- `httpClient` (optional): Custom HTTP client

### Methods

```csharp
// Simple send
Task<NotificationResponse> SendAsync(
    string title,
    string message,
    CancellationToken cancellationToken = default)

// Advanced send with all parameters
Task<NotificationResponse> SendNotificationAsync(
    Notification notification,
    CancellationToken cancellationToken = default)
```

### Notification

**Properties:**
- `Title` (string, required): Notification title
- `Message` (string, required): Notification message
- `Type` (string, optional): Category for organization
- `Tags` (string[], optional): Tags for filtering
- `ImageUrl` (string, optional): Image URL to display
- `ActionUrl` (string, optional): URL to open when tapped
- `EncryptionPassword` (string, optional): Password for encryption

### NotificationResponse

**Properties:**
- `Status` (string): Response status (e.g., "success")
- `Message` (string): Response message
- `IsSuccess` (bool): Returns true if status is "success"

### Exceptions

- `WirePusherException`: Base exception for all SDK errors
- `AuthenticationException`: Invalid token (401, 403)
- `ValidationException`: Invalid parameters (400, 404)
- `RateLimitException`: Rate limit exceeded (429)

All exceptions include:
- `Message`: Error description
- `StatusCode`: HTTP status code
- `InnerException`: Original cause if available

## Error Handling

```csharp
using WirePusher;
using WirePusher.Exceptions;

var client = new WirePusherClient(
    Environment.GetEnvironmentVariable("WIREPUSHER_TOKEN")!);

try
{
    await client.SendAsync("Deploy Complete", "Version 1.2.3 deployed");
}
catch (AuthenticationException ex)
{
    Console.WriteLine($"Authentication failed: {ex.Message}");
}
catch (ValidationException ex)
{
    Console.WriteLine($"Invalid parameters: {ex.Message}");
}
catch (WirePusherException ex)
{
    Console.WriteLine($"API error: {ex.Message}");
}
```

## Examples

### CI/CD Pipeline

```csharp
using WirePusher;

public class DeploymentNotifier
{
    private readonly IWirePusherClient _client;

    public DeploymentNotifier()
    {
        var token = Environment.GetEnvironmentVariable("WIREPUSHER_TOKEN")!;
        _client = new WirePusherClient(token);
    }

    public async Task NotifyDeploymentAsync(string version, string environment)
    {
        var notification = new Notification
        {
            Title = "Deploy Complete",
            Message = $"Version {version} deployed to {environment}",
            Type = "deployment",
            Tags = new[] { environment, version }
        };

        await _client.SendNotificationAsync(notification);
    }
}

// In your deployment script
var notifier = new DeploymentNotifier();
await notifier.NotifyDeploymentAsync("1.2.3", "production");
```

### Server Monitoring

```csharp
using WirePusher;
using System.Diagnostics;

public class ServerMonitor
{
    private readonly IWirePusherClient _client;

    public ServerMonitor(IWirePusherClient client)
    {
        _client = client;
    }

    public async Task CheckServerHealthAsync()
    {
        var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        var cpu = cpuCounter.NextValue();

        if (cpu > 80)
        {
            await _client.SendAsync(
                "Server Alert",
                $"CPU usage is at {cpu:F1}%",
                new CancellationToken());
        }
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
    return new WirePusherClient(config["WirePusher:Token"]!);
});

var app = builder.Build();

app.MapPost("/deploy", async (
    string version,
    IWirePusherClient client) =>
{
    try
    {
        var notification = new Notification
        {
            Title = "Deploy Complete",
            Message = $"Version {version} deployed to production",
            Type = "deployment",
            Tags = new[] { "production", "backend" }
        };

        var response = await client.SendNotificationAsync(notification);
        return Results.Ok(response);
    }
    catch (WirePusherException ex)
    {
        return Results.Problem(ex.Message, statusCode: ex.StatusCode);
    }
});

app.Run();
```

### Background Service

```csharp
using WirePusher;
using Microsoft.Extensions.Hosting;

public class NotificationBackgroundService : BackgroundService
{
    private readonly IWirePusherClient _client;
    private readonly ILogger<NotificationBackgroundService> _logger;

    public NotificationBackgroundService(
        IWirePusherClient client,
        ILogger<NotificationBackgroundService> logger)
    {
        _client = client;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Your monitoring logic here
                await _client.SendAsync(
                    "Deploy Complete",
                    "Version 1.2.3 deployed to production",
                    stoppingToken);
            }
            catch (WirePusherException ex)
            {
                _logger.LogError(ex, "Failed to send notification");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

## Development

### Setup

```bash
# Clone repository
git clone https://gitlab.com/wirepusher/csharp-sdk.git
cd csharp-sdk

# Restore dependencies
dotnet restore
```

### Building

```bash
dotnet build
```

### Testing

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# View coverage report (requires reportgenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
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

## Requirements

- .NET 6.0 or higher

## Links

- **Documentation**: https://wirepusher.dev/help
- **Repository**: https://gitlab.com/wirepusher/csharp-sdk
- **Issues**: https://gitlab.com/wirepusher/csharp-sdk/-/issues
- **NuGet**: https://www.nuget.org/packages/WirePusher/

## License

MIT License - see [LICENSE](LICENSE) file for details.
