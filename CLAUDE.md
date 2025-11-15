# CLAUDE.md - WirePusher C# Client Library

Context file for AI-powered development assistance on the WirePusher C# Client Library project.

## Project Overview

**WirePusher C# Client Library** is a .NET client library for sending push notifications via [WirePusher](https://wirepusher.dev).

- **Language**: C# (.NET 6+)
- **Framework**: Built-in HttpClient and System.Text.Json (zero external dependencies)
- **Purpose**: Send notifications from .NET applications, ASP.NET Core services, and background workers
- **Philosophy**: Simple, idiomatic C#, zero external dependencies, async-first

## Architecture

```
wirepusher-csharp/
├── src/WirePusher/              # Main library code
│   ├── WirePusher.csproj        # Project file (targets net8.0)
│   ├── WirePusherClient.cs      # Main client implementation
│   ├── IWirePusherClient.cs     # Interface for dependency injection
│   ├── Notification.cs          # Notification model (request)
│   ├── NotificationResponse.cs  # Response model
│   ├── NotifAIRequest.cs        # NotifAI request model
│   ├── ErrorResponse.cs         # Error response model
│   ├── Crypto/
│   │   └── EncryptionUtil.cs    # AES-128-CBC encryption
│   ├── Validation/
│   │   └── TagNormalizer.cs     # Tag normalization/validation
│   └── Exceptions/              # Custom exceptions
│       ├── WirePusherException.cs       # Base exception
│       ├── AuthenticationException.cs   # 401, 403
│       ├── ValidationException.cs       # 400, 404
│       ├── RateLimitException.cs        # 429
│       ├── ServerException.cs           # 5xx
│       └── NetworkException.cs          # Network/timeout errors
├── tests/WirePusher.Tests/      # Test suite (xUnit)
│   ├── WirePusherClientTests.cs # Client tests
│   ├── NotificationTests.cs     # Model tests
│   └── ExceptionTests.cs        # Exception tests
├── examples/                    # Usage examples
├── docs/                        # Documentation
└── README.md                    # Main documentation
```

## Key Features

### 1. Async-First Architecture

**WirePusherClient** - Full async/await support:
- All operations are async with CancellationToken support
- Thread-safe implementation
- Interface-based for dependency injection
- Follows .NET async best practices

```csharp
var client = new WirePusherClient("wpt_abc123", null);
await client.SendAsync("Title", "Message", cancellationToken);
```

### 2. API Endpoints

**SendAsync()** - Send notifications:
```csharp
var notification = new Notification
{
    Title = "Deploy Complete",
    Message = "v1.2.3 deployed",
    Type = "deployment",
    Tags = new[] { "production", "release" },
    ImageUrl = "https://example.com/image.png",
    ActionUrl = "https://example.com/action",
    EncryptionPassword = "secret"  // Optional
};
await client.SendNotificationAsync(notification);
```

**NotifAIAsync()** - AI-powered notifications:
```csharp
await client.NotifAIAsync("deployment finished successfully, v2.1.3 is live on prod");

// With type override
var request = new NotifAIRequest
{
    Input = "deployment finished successfully, v2.1.3 is live on prod",
    Type = "deployment"
};
await client.NotifAIAsync(request);
```

### 3. Automatic Retry Logic

- **Default**: 3 retries with exponential backoff (1s, 2s, 4s, 8s)
- **Configurable**: `maxRetries` parameter (0 to disable)
- **Retryable errors**: Network errors, 5xx, 429 (rate limit)
- **Non-retryable**: 400, 401, 403, 404 (client errors)
- **Rate limit handling**: Special backoff (5s, 10s, 20s)

Implementation: `ExecuteWithRetryAsync()` and `DelayWithBackoffAsync()` in `WirePusherClient.cs`

### 4. Error Categorization

All exceptions inherit from `WirePusherException` with `IsRetryable` property:

- `AuthenticationException` (401, 403) → `IsRetryable = false`
- `ValidationException` (400, 404) → `IsRetryable = false`
- `RateLimitException` (429) → `IsRetryable = true`
- `ServerException` (5xx) → `IsRetryable = true`
- `NetworkException` (network/timeout) → `IsRetryable = true`

```csharp
try
{
    await client.SendAsync("Title", "Message");
}
catch (AuthenticationException ex)
{
    // Invalid token - don't retry
    Console.WriteLine($"Auth failed: {ex.Message}");
}
catch (RateLimitException ex)
{
    // Rate limited - automatically retried
    Console.WriteLine($"Rate limited: {ex.Message}");
}
```

### 5. Tag Normalization

Automatic validation and normalization:
- Lowercase conversion
- Whitespace trimming
- Duplicate removal (case-insensitive)
- Character validation (alphanumeric, hyphens, underscores only)
- Invalid tags filtered out

Implementation: `TagNormalizer.cs`

### 6. Dependency Injection Support

Interface-based design for ASP.NET Core:

```csharp
// Program.cs
builder.Services.AddSingleton<IWirePusherClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new WirePusherClient(config["WirePusher:Token"]!, null);
});

// Service
public class NotificationService
{
    private readonly IWirePusherClient _client;

    public NotificationService(IWirePusherClient client)
    {
        _client = client;
    }
}
```

### 7. Encryption

**AES-128-CBC** encryption matching mobile app:
- SHA1-based key derivation (for app compatibility)
- Custom Base64 encoding (URL-safe: `-` `_` `.` instead of `+` `/` `=`)
- Only message/input encrypted (title, type, tags remain visible)
- Password must match type configuration in app

Implementation: `EncryptionUtil.cs`

## Configuration Priority

Constructor-based configuration (no config file support needed):

```csharp
// Required
var client = new WirePusherClient("wpt_abc123", null);

// Custom timeout
var client = new WirePusherClient("wpt_abc123", null, TimeSpan.FromSeconds(60));

// Custom HttpClient (for testing)
var client = new WirePusherClient("wpt_abc123", null, httpClient);

// Custom max retries
var client = new WirePusherClient("wpt_abc123", null, httpClient, maxRetries: 5);
```

## Dependencies

**Runtime** (zero external dependencies):
- .NET 6+ built-in libraries only
- `System.Net.Http` - HttpClient
- `System.Text.Json` - JSON serialization
- `System.Security.Cryptography` - AES encryption

**Development**:
- `xUnit >= 2.4.0` - Testing framework
- `Moq >= 4.18.0` - Mocking library
- `Microsoft.NET.Test.Sdk` - Test infrastructure
- `coverlet.collector` - Code coverage

## Recent Changes

### v1.0.0 (Current)

**Added**:
- Initial release of WirePusher C# Client Library
- NotifAI endpoint for AI-powered notifications
- Automatic retry logic with exponential backoff
- Enhanced error categorization (`RateLimitException`, `ServerException`, `NetworkException`)
- Tag normalization and validation
- AES-128-CBC encryption support
- Interface-based design (`IWirePusherClient`) for dependency injection
- Full async/await support with CancellationToken
- Nullable reference types enabled
- XML documentation for IntelliSense

**Technical**:
- Zero external dependencies (uses .NET 6+ built-in libraries)
- Modern C# features: records, init-only properties, pattern matching
- Thread-safe implementation
- 90%+ test coverage target

## Development

### Setup

```bash
# Clone repository
git clone https://gitlab.com/wirepusher/wirepusher-csharp.git
cd wirepusher-csharp

# Restore dependencies
dotnet restore
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

### Code Quality

```bash
# Build
dotnet build

# Build with warnings as errors
dotnet build -p:TreatWarningsAsErrors=true

# Format code
dotnet format

# Run all checks
dotnet build -p:TreatWarningsAsErrors=true && dotnet test
```

### Type Safety

Full nullable reference types support:
- All public APIs have nullable annotations
- XML documentation for all public members
- IntelliSense support in Visual Studio/Rider
- Compiler warnings for null-safety violations

## Common Development Tasks

### Adding a Feature

1. Implement in `WirePusherClient.cs`
2. Add to `IWirePusherClient.cs` interface
3. Add tests in `tests/WirePusher.Tests/`
4. Update README with examples
5. Add to CHANGELOG

### Adding an Exception

1. Add to `Exceptions/` folder
2. Inherit from `WirePusherException`
3. Set `IsRetryable` property
4. Update `HandleResponseAsync()` in WirePusherClient
5. Update README error handling section

### Adding Validation

1. Add logic to `Validation/` folder
2. Write comprehensive tests
3. Integrate into client (`WirePusherClient.cs`)
4. Document behavior in README

## Testing Philosophy

- **Unit tests**: Test classes in isolation
- **Mock HTTP**: Use custom HttpClient for HTTP mocking
- **Async tests**: All tests use async/await patterns
- **Null safety**: Test nullable reference type behavior
- **Coverage target**: 90%+ for critical paths

## API Integration

### Endpoints

- `POST /send` - Send notifications
- `POST /notifai` - AI-generated notifications

### Authentication

Token via constructor parameter only (no config file needed).

### Response Format

**Success response:**
```json
{
  "status": "success",
  "message": "Notification sent successfully"
}
```

**Error response:**
```json
{
  "status": "error",
  "error": {
    "type": "validation_error",
    "code": "missing_required_field",
    "message": "Title is required",
    "param": "title"
  }
}
```

The library expects and parses the nested error format. It extracts error details and builds descriptive error messages:
- `error.message` - Human-readable error message
- `error.code` - Machine-readable error code (appended in brackets)
- `error.type` - Error category (authentication_error, validation_error, etc.)
- `error.param` - Parameter that caused the error (appended in parentheses)

Example: `Invalid parameters: Title is required [missing_required_field] (parameter: title)`

## Notes for AI Assistants

- **Simplicity is key**: Keep API as simple as the WirePusher app
- **Idiomatic C#**: Follow .NET conventions, use modern C# features
- **Async-first**: All I/O operations are async with CancellationToken support
- **Zero external deps**: Use only .NET 6+ built-in libraries
- **Interface-based**: Use `IWirePusherClient` for dependency injection
- **Null safety**: Enable nullable reference types, annotate all APIs
- **XML docs**: Include XML documentation for IntelliSense
- **Test coverage**: Aim for 90%+ on critical paths
- **Documentation**: Update README for user-facing changes
- **Consistency**: Match CLI/Python features when applicable
- **NuGet packaging**: Keep metadata updated in .csproj

## Project Status

**Current**: Production-ready v1.0.0 with comprehensive feature set

**Completed**:
- ✅ NotifAI endpoint
- ✅ Automatic retry logic
- ✅ Enhanced error categorization
- ✅ Tag normalization
- ✅ AES-128-CBC encryption
- ✅ Interface-based design for DI
- ✅ Zero external dependencies
- ✅ Comprehensive documentation
- ✅ Async-first architecture
- ✅ Nullable reference types

**Not Needed**:
- ❌ Config file support (not standard for .NET libraries)
- ❌ CLI tool (separate wirepusher-cli project)

## Links

- **Repository**: https://gitlab.com/wirepusher/wirepusher-csharp
- **Issues**: https://gitlab.com/wirepusher/wirepusher-csharp/-/issues
- **NuGet**: https://www.nuget.org/packages/WirePusher/
- **API Docs**: https://wirepusher.com/docs
- **App**: https://wirepusher.dev
