# Advanced Documentation

Advanced configuration, patterns, and API reference for the Pincho C# Client Library.

## Table of Contents

- [API Reference](#api-reference)
- [ASP.NET Core Patterns](#aspnet-core-patterns)
- [Encryption](#encryption)
- [Tag Normalization](#tag-normalization)
- [Retry Logic](#retry-logic)
- [Examples](#examples)

## API Reference

### PinchoClient

Main client class implementing `IPinchoClient`.

```csharp
// Constructor variants
public PinchoClient(string token)
public PinchoClient(string token, TimeSpan timeout)
public PinchoClient(string token, HttpClient httpClient)
public PinchoClient(string token, HttpClient httpClient, int maxRetries)
```

**Parameters:**
- `token` - Pincho API token (required)
- `timeout` - Request timeout (default: 30 seconds)
- `httpClient` - Custom HttpClient instance (for testing or advanced scenarios)
- `maxRetries` - Maximum retry attempts (default: 3)

**Authentication:** Token is passed via Bearer token in the Authorization header.

### Methods

#### SendAsync

Simple send with title and message only.

```csharp
Task<NotificationResponse> SendAsync(
    string title,
    string message,
    CancellationToken cancellationToken = default)
```

#### SendNotificationAsync

Full-featured send with all options.

```csharp
Task<NotificationResponse> SendNotificationAsync(
    Notification notification,
    CancellationToken cancellationToken = default)
```

#### NotifAIAsync

AI-powered notification generation.

```csharp
// Simple text input
Task<NotificationResponse> NotifAIAsync(
    string input,
    CancellationToken cancellationToken = default)

// Full options with type override
Task<NotificationResponse> NotifAIAsync(
    NotifAIRequest request,
    CancellationToken cancellationToken = default)
```

### Models

#### Notification

Request model for sending notifications.

```csharp
public record Notification
{
    public required string Title { get; init; }       // Required
    public required string Message { get; init; }     // Required
    public string? Type { get; init; }                // Optional
    public string[]? Tags { get; init; }              // Optional (auto-normalized)
    public string? ImageUrl { get; init; }            // Optional
    public string? ActionUrl { get; init; }           // Optional
    public string? EncryptionPassword { get; init; }  // Optional
}
```

#### NotifAIRequest

Request model for AI-powered notifications.

```csharp
public record NotifAIRequest
{
    public required string Text { get; init; }        // Required - natural language input
    public string? Type { get; init; }                // Optional - override AI-selected type
    public string? EncryptionPassword { get; init; }  // Optional
}
```

#### NotificationResponse

Response from the API.

```csharp
public record NotificationResponse
{
    public string Status { get; init; }   // "success" or "error"
    public string Message { get; init; }  // Human-readable message
}
```

### Exceptions

All exceptions inherit from `PinchoException` and include an `IsRetryable` property.

| Exception | Status Code | IsRetryable | Description |
|-----------|-------------|-------------|-------------|
| `AuthenticationException` | 401, 403 | false | Invalid or expired token |
| `ValidationException` | 400, 404 | false | Invalid request parameters |
| `RateLimitException` | 429 | true | Rate limit exceeded |
| `ServerException` | 5xx | true | Server-side error |
| `NetworkException` | N/A | true | Network or timeout error |
| `PinchoException` | Other | varies | Base exception class |

## ASP.NET Core Patterns

### Dependency Injection

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IPinchoClient>(sp =>
{
    var token = builder.Configuration["Pincho:Token"]
        ?? throw new InvalidOperationException("Pincho token not configured");
    return new PinchoClient(token);
});
```

### Configuration via appsettings.json

```json
{
  "Pincho": {
    "Token": "abc12345"
  }
}
```

### Controller Pattern

```csharp
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly IPinchoClient _client;

    public NotificationsController(IPinchoClient client)
    {
        _client = client;
    }

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] NotifyRequest request)
    {
        try
        {
            var response = await _client.SendAsync(request.Title, request.Message);
            return Ok(response);
        }
        catch (AuthenticationException)
        {
            return StatusCode(503, "Notification service unavailable");
        }
        catch (PinchoException ex)
        {
            return StatusCode(500, $"Failed to send notification: {ex.Message}");
        }
    }
}
```

### Background Service

```csharp
public class AlertMonitorService : BackgroundService
{
    private readonly IPinchoClient _client;
    private readonly ILogger<AlertMonitorService> _logger;

    public AlertMonitorService(IPinchoClient client, ILogger<AlertMonitorService> logger)
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
                // Check for alerts...
                if (AlertDetected())
                {
                    await _client.SendAsync(
                        "System Alert",
                        "Critical condition detected",
                        stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process alerts");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
```

### Minimal API

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IPinchoClient>(
    _ => new PinchoClient(builder.Configuration["Pincho:Token"]!));

var app = builder.Build();

app.MapPost("/notify", async (
    IPinchoClient client,
    NotifyRequest request) =>
{
    await client.SendAsync(request.Title, request.Message);
    return Results.Ok();
});

app.Run();

record NotifyRequest(string Title, string Message);
```

## Encryption

Pincho supports AES-128-CBC encryption for message privacy.

### How It Works

1. **Key Derivation**: SHA1(password) → first 32 hex chars → 16 bytes
2. **IV Generation**: 16 random bytes per message
3. **Encryption**: AES-128-CBC with PKCS7 padding
4. **Encoding**: Custom Base64 (URL-safe: `+` → `-`, `/` → `.`, `=` → `_`)
5. **Transmission**: Encrypted message + IV sent to API

### What's Encrypted

- **Message content** - Encrypted ✅
- **NotifAI input** - Encrypted ✅
- **Title** - NOT encrypted ❌
- **Type** - NOT encrypted ❌
- **Tags** - NOT encrypted ❌
- **URLs** - NOT encrypted ❌

### Example

```csharp
var notification = new Notification
{
    Title = "Security Alert",               // Visible
    Message = "Sensitive data here",        // Encrypted
    Type = "security",                      // Visible
    Tags = new[] { "critical" },            // Visible
    EncryptionPassword = "my_password_123"  // Must match app config
};

await client.SendNotificationAsync(notification);
```

### Mobile App Configuration

To receive encrypted messages:

1. Open Pincho app
2. Go to Settings → Notification Types
3. Select or create a notification type
4. Enable encryption and set the same password

## Tag Normalization

Tags are automatically normalized before sending:

1. **Lowercase** - `PROD` → `prod`
2. **Trim** - `"  prod  "` → `"prod"`
3. **Remove invalid chars** - `prod@123!` → `prod123`
4. **Deduplicate** - Case-insensitive deduplication
5. **Filter empty** - Empty tags removed
6. **Limit** - Max 10 tags enforced

### Valid Characters

- Lowercase letters: `a-z`
- Numbers: `0-9`
- Hyphens: `-`
- Underscores: `_`

### Example

```csharp
var notification = new Notification
{
    Title = "Test",
    Message = "Testing tags",
    Tags = new[]
    {
        "  PROD  ",           // → "prod"
        "Backend@123!",       // → "backend123"
        "test-env_1",         // → "test-env_1"
        "PROD",               // → removed (duplicate)
        "   ",                // → removed (empty)
        "special!@#$chars"    // → "specialchars"
    }
};

// Final tags: ["prod", "backend123", "test-env_1", "specialchars"]
```

## Retry Logic

The client automatically retries failed requests with exponential backoff.

### Configuration

```csharp
// Default: 3 retries
var client = new PinchoClient("token");

// Custom: 5 retries
var httpClient = new HttpClient { BaseAddress = new Uri("https://api.pincho.app/") };
httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer token");
var client = new PinchoClient("token", httpClient, maxRetries: 5);

// Disable retries
var client = new PinchoClient("token", httpClient, maxRetries: 0);
```

### Retry Behavior

| Error Type | Retried? | Backoff Strategy |
|------------|----------|------------------|
| Network/Timeout | Yes | 1s, 2s, 4s, 8s... |
| 429 Rate Limit | Yes | Retry-After header or 5s, 10s, 20s... |
| 5xx Server | Yes | 1s, 2s, 4s, 8s... |
| 400 Validation | No | Immediate failure |
| 401/403 Auth | No | Immediate failure |

**Maximum delay**: Capped at 30 seconds.

**Retry-After Header**: The client respects `Retry-After` headers from rate limit responses for precise backoff timing.

### Error Propagation

After exhausting retries, the appropriate exception is thrown:

```csharp
try
{
    await client.SendAsync("Title", "Message");
}
catch (NetworkException ex)
{
    // Failed after all retry attempts
    Console.WriteLine($"Network error after retries: {ex.Message}");
}
catch (ServerException ex)
{
    // Server error persisted after retries
    Console.WriteLine($"Server error after retries: {ex.Message}");
}
```

## Examples

### CI/CD Pipeline Notifications

```csharp
public class BuildNotifier
{
    private readonly IPinchoClient _client;

    public BuildNotifier(IPinchoClient client)
    {
        _client = client;
    }

    public async Task NotifyBuildCompleteAsync(BuildResult result)
    {
        var notification = new Notification
        {
            Title = result.Success ? "Build Passed ✅" : "Build Failed ❌",
            Message = $"Pipeline #{result.PipelineId} {(result.Success ? "succeeded" : "failed")} in {result.Duration}",
            Type = result.Success ? "build_success" : "build_failure",
            Tags = new[] { result.Branch, result.Environment },
            ActionUrl = result.LogUrl
        };

        await _client.SendNotificationAsync(notification);
    }
}
```

### Server Monitoring

```csharp
public class HealthMonitor
{
    private readonly IPinchoClient _client;
    private readonly ILogger _logger;

    public async Task CheckServerHealthAsync()
    {
        var metrics = await GetMetricsAsync();

        if (metrics.CpuUsage > 90)
        {
            await _client.SendNotificationAsync(new Notification
            {
                Title = "High CPU Usage",
                Message = $"CPU at {metrics.CpuUsage}% on {metrics.ServerName}",
                Type = "alert",
                Tags = new[] { "cpu", "critical", metrics.ServerName },
                ImageUrl = $"https://monitoring.example.com/cpu/{metrics.ServerName}.png"
            });
        }

        if (metrics.MemoryUsage > 85)
        {
            await _client.SendNotificationAsync(new Notification
            {
                Title = "Memory Warning",
                Message = $"Memory at {metrics.MemoryUsage}% on {metrics.ServerName}",
                Type = "warning",
                Tags = new[] { "memory", "warning", metrics.ServerName }
            });
        }

        if (metrics.DiskUsage > 95)
        {
            await _client.SendNotificationAsync(new Notification
            {
                Title = "Disk Space Critical",
                Message = $"Disk at {metrics.DiskUsage}% on {metrics.ServerName}. Only {metrics.DiskFree}GB remaining.",
                Type = "alert",
                Tags = new[] { "disk", "critical", metrics.ServerName },
                ActionUrl = $"https://admin.example.com/disk/{metrics.ServerName}"
            });
        }
    }
}
```

### Using NotifAI

Let AI generate the notification structure:

```csharp
public class AINotifier
{
    private readonly IPinchoClient _client;

    public async Task NotifyWithAIAsync(string eventDescription)
    {
        // AI will generate appropriate title, message, and tags
        var response = await _client.NotifAIAsync(eventDescription);

        Console.WriteLine($"AI generated notification: {response.Message}");
    }

    public async Task NotifyWithTypeOverrideAsync(string eventDescription, string type)
    {
        // AI generates content, but uses specified type
        var request = new NotifAIRequest
        {
            Text = eventDescription,
            Type = type  // Force specific type
        };

        await _client.NotifAIAsync(request);
    }
}

// Usage
await notifier.NotifyWithAIAsync("deployment completed successfully, version 2.1.3 is now live on production");
await notifier.NotifyWithTypeOverrideAsync("server running out of disk space, only 5% remaining", "alert");
```

## Development

### Building

```bash
dotnet build
dotnet build -p:TreatWarningsAsErrors=true
```

### Testing

```bash
dotnet test
dotnet test --collect:"XPlat Code Coverage"
```

### Project Structure

```
pincho-csharp/
├── src/Pincho/
│   ├── PinchoClient.cs           # Main client
│   ├── IPinchoClient.cs          # Interface for DI
│   ├── Notification.cs               # Request model
│   ├── NotifAIRequest.cs             # AI request model
│   ├── NotificationResponse.cs       # Response model
│   ├── Crypto/
│   │   └── EncryptionUtil.cs         # AES-128-CBC encryption
│   ├── Validation/
│   │   └── TagNormalizer.cs          # Tag processing
│   └── Exceptions/
│       ├── PinchoException.cs
│       ├── AuthenticationException.cs
│       ├── ValidationException.cs
│       ├── RateLimitException.cs
│       ├── ServerException.cs
│       └── NetworkException.cs
└── tests/Pincho.Tests/
    ├── PinchoClientTests.cs
    ├── TagNormalizerTests.cs
    └── EncryptionUtilTests.cs
```

## Further Reading

- [README.md](../README.md) - Quick start guide
- [SECURITY.md](SECURITY.md) - Security policy
- [Pincho Documentation](https://pincho.app/help)
