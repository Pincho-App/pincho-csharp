# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2025-10-26

### Added
- Initial release of WirePusher C# SDK
- `WirePusherClient` class with async `SendAsync()` methods
- `IWirePusherClient` interface for dependency injection
- .NET 6+ with built-in `HttpClient` and `System.Text.Json` (zero external dependencies)
- Full async/await support with `CancellationToken`
- Nullable reference types enabled
- Modern C# features: records, init-only properties, pattern matching
- Custom exceptions for better error handling:
  - `WirePusherException` - Base exception
  - `AuthenticationException` - Auth failures (401, 403)
  - `ValidationException` - Invalid parameters (400, 404)
  - `RateLimitException` - Rate limit errors (429)
- Comprehensive test suite (xUnit, >90% coverage target)
- Support for all v1 API parameters:
  - `Title` and `Message` (required)
  - `Type` for notification categorization
  - `Tags` for flexible organization (up to 10 tags)
  - `ImageUrl` for visual attachments
  - `ActionUrl` for tap actions
- Thread-safe implementation
- Complete XML documentation for IntelliSense
- Comprehensive documentation:
  - README with quickstart guide
  - API reference
  - Error handling guide
  - Usage examples (basic, async, ASP.NET Core DI, minimal API)
- MIT License

### Dependencies
- None for runtime (uses built-in .NET 6+ libraries)
- .NET 6+ built-in HttpClient (no external HTTP library required)
- System.Text.Json (built-in, no Newtonsoft.Json required)

[Unreleased]: https://gitlab.com/wirepusher/csharp-sdk/-/compare/v1.0.0...main
[1.0.0]: https://gitlab.com/wirepusher/csharp-sdk/-/tags/v1.0.0
