# CLAUDE.md - Pincho C# Library

## Overview

.NET client for Pincho push notifications. .NET 8+, zero external dependencies.

## Structure

```
src/Pincho/
├── PinchoClient.cs         # Main client
├── IPinchoClient.cs        # Interface for DI
├── Notification.cs         # Send parameters
├── NotifAIRequest.cs       # NotifAI request
├── NotificationResponse.cs
├── ErrorResponse.cs
├── Crypto/EncryptionUtil.cs
├── Validation/TagNormalizer.cs
└── Exceptions/             # Error hierarchy
```

## API

```csharp
var client = new PinchoClient("TOKEN");
await client.SendAsync("Title", "Message");
await client.SendNotificationAsync(new Notification {...});
await client.NotifAIAsync("text");
await client.NotifAIAsync(new NotifAIRequest {...});
```

## Exceptions

- `AuthenticationException` (401/403) - not retryable
- `ValidationException` (400/404) - not retryable
- `RateLimitException` (429) - retryable
- `ServerException` (5xx) - retryable
- `NetworkException` (connection) - retryable

All have `IsRetryable` property.

## Development

```bash
dotnet test              # Run tests
dotnet build             # Build
dotnet pack              # Create NuGet package
```

## Notes

- Zero external dependencies
- Bearer token auth
- Auto retry with exponential backoff
- Tag normalization (lowercase, trim, dedupe)
- Encryption: AES-128-CBC with separate IV field
