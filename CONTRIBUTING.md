# Contributing to WirePusher C# Client Library

Thanks for considering contributing! This is a small project with a small team, so every contribution makes a real difference.

## Code of Conduct

Be respectful and constructive. See [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) for details.

## Quick Start

```bash
# Get the code
git clone https://github.com/Pincho-App/pincho-csharp.git
cd pincho-csharp

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run tests
dotnet test

# Make sure everything passes
dotnet test --collect:"XPlat Code Coverage"
dotnet build -p:TreatWarningsAsErrors=true
```

**Requirements:** .NET 6.0 or higher

## How to Contribute

### Report a Bug

[Check existing issues](https://github.com/Pincho-App/pincho-csharp/issues) first, then create a new one with:

- What you did (exact code)
- What happened vs what you expected
- Your environment (SDK version, .NET version, OS)
- Error output and stack trace

**Example:**
```
## Retry logic doesn't work for 429 errors

**Code:**
```csharp
using WirePusher;

var client = new WirePusherClient("abc12345", null);
await client.SendAsync("Title", "Message");
```

**Expected:** Retries with backoff
**Actual:** Raises RateLimitException immediately
**Environment:** SDK v1.0.0, .NET 8.0, Ubuntu 22.04

[Paste error output]
```

### Suggest a Feature

Create an issue explaining:
- What you want and why
- How it would work
- Any alternatives you considered

### Submit Code

1. **Fork** the repo
2. **Create a branch**: `git checkout -b fix-something`
3. **Make your changes**
4. **Add tests** for new functionality
5. **Run tests**: `dotnet test --collect:"XPlat Code Coverage"`
6. **Format**: `dotnet format`
7. **Build with warnings as errors**: `dotnet build -p:TreatWarningsAsErrors=true`
8. **Commit** with a clear message
9. **Push** to your fork
10. **Open a Merge Request**

## Code Guidelines

- Follow [C# coding conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use async/await for all I/O operations
- Enable nullable reference types
- Add XML documentation comments for public APIs
- Keep methods small and focused
- Update README for user-facing changes

**Good commit messages:**
```
Add retry logic for network errors
Fix tag validation edge case
Update docs with async examples
```

**Bad commit messages:**
```
fix bug
update
changes
```

## Project Structure

```
src/WirePusher/   # Library code (client, models, exceptions, crypto, validation)
tests/            # Test suite (xUnit)
examples/         # Usage examples
docs/             # Documentation
```

See [CLAUDE.md](CLAUDE.md) for details.

## Common Tasks

**Add a feature:** Update `WirePusherClient.cs`, add to `IWirePusherClient.cs`, write tests

**Add validation:** Add logic in `Validation/` folder, write tests, integrate into client

**Add exception:** Add to `Exceptions/` folder, inherit from `WirePusherException`, update error handling

## Testing

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test file
dotnet test --filter "FullyQualifiedName~WirePusherClientTests"

# Run specific test
dotnet test --filter "FullyQualifiedName~SendAsync_WithValidParameters_ReturnsSuccess"
```

## Code Quality

```bash
# Build
dotnet build

# Build with warnings as errors
dotnet build -p:TreatWarningsAsErrors=true

# Format code
dotnet format

# Generate coverage report (requires reportgenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport"

# All checks
dotnet restore && dotnet build -p:TreatWarningsAsErrors=true && dotnet test --collect:"XPlat Code Coverage"
```

## Need Help?

- **Architecture questions?** See [CLAUDE.md](CLAUDE.md)
- **Stuck?** Open an issue or ask in your MR

## License

By contributing, you agree your contributions will be licensed under the MIT License.

---

Thanks for contributing!
