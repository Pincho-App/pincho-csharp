# WirePusher C# Examples

Working examples demonstrating the WirePusher C# client library.

## Prerequisites

- .NET 8.0+
- WirePusher token (App → Settings → Help → copy token)

## Examples

### Basic
Simple notification sending with title and message.
```bash
cd Basic
dotnet run
```

### Advanced
Full notification with type, tags, image, and action URL.
```bash
cd Advanced
dotnet run
```

### NotifAI
AI-powered notification generation from free-form text.
```bash
cd NotifAI
dotnet run
```

### ErrorHandling
Exception handling patterns for different error types.
```bash
cd ErrorHandling
dotnet run
```

### Encryption
AES-128-CBC encrypted message sending.
```bash
cd Encryption
dotnet run
```

### AspNetCore
Dependency injection integration for ASP.NET Core applications.
```bash
cd AspNetCore
dotnet run
```

### RateLimits
Rate limit monitoring and tracking.
```bash
cd RateLimits
dotnet run
```

## Running Examples

Each example is a standalone .NET console application:

```bash
# Set your token (or modify Program.cs)
export WIREPUSHER_TOKEN=your_token_here

# Run any example
cd Basic
dotnet run
```

## Notes

- All examples use `WIREPUSHER_TOKEN` environment variable by default
- Modify `Program.cs` in each example to customize behavior
- Examples reference the local `WirePusher` package via project reference
