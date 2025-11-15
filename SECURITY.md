# Security Policy

## Supported Versions

We release patches for security vulnerabilities in the following versions:

| Version | Supported          |
| ------- | ------------------ |
| 1.x.x   | :white_check_mark: |
| < 1.0   | :x:                |

## Reporting a Vulnerability

The WirePusher team takes security bugs seriously. We appreciate your efforts to responsibly disclose your findings.

### How to Report

**Please do NOT report security vulnerabilities through public GitLab issues.**

Instead, please report security vulnerabilities via email to:

**security@wirepusher.com**

### What to Include

To help us triage and fix the issue quickly, please include:

1. **Type of vulnerability** (e.g., authentication bypass, injection, etc.)
2. **Full paths** of source files related to the vulnerability
3. **Location** of the affected source code (tag/branch/commit or direct URL)
4. **Step-by-step instructions** to reproduce the issue
5. **Proof-of-concept or exploit code** (if possible)
6. **Impact** of the vulnerability (what an attacker could achieve)
7. **Any mitigating factors** or workarounds you've identified

### What to Expect

After you submit a report:

1. **Acknowledgment** - We'll acknowledge receipt within 48 hours
2. **Assessment** - We'll assess the vulnerability and determine severity
3. **Updates** - We'll provide regular updates (at least every 7 days)
4. **Fix Timeline** - We aim to release fixes for:
   - **Critical** vulnerabilities: Within 7 days
   - **High** vulnerabilities: Within 14 days
   - **Medium** vulnerabilities: Within 30 days
   - **Low** vulnerabilities: Next regular release

5. **Disclosure** - We'll coordinate with you on public disclosure timing
6. **Credit** - We'll credit you in the security advisory (unless you prefer to remain anonymous)

## Security Best Practices

### For Users

When using the WirePusher C# Client Library:

1. **Keep the SDK updated** to the latest version
2. **Never commit credentials** to version control
3. **Use environment variables** for sensitive configuration
4. **Validate input** before sending to the SDK
5. **Handle errors gracefully** without exposing sensitive information
6. **Use HTTPS** for all network communication
7. **Limit token scope** to minimum required permissions

### Credential Management

```csharp
// ❌ Bad - Hardcoded credentials
var client = new WirePusherClient("abc12345", null);

// ✅ Good - Environment variables
var token = Environment.GetEnvironmentVariable("WIREPUSHER_TOKEN");
var client = new WirePusherClient(token!, null);

// ✅ Good - Configuration (ASP.NET Core)
var token = configuration["WirePusher:Token"];
var client = new WirePusherClient(token!, null);
```

### Error Handling

```csharp
// ❌ Bad - Exposes sensitive information
try
{
    await client.SendAsync("Title", "Message");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex}"); // May log tokens or sensitive data
}

// ✅ Good - Safe error handling
try
{
    await client.SendAsync("Title", "Message");
}
catch (AuthenticationException ex)
{
    logger.LogError("Authentication failed - check credentials");
}
catch (ValidationException ex)
{
    logger.LogError("Validation error: {Message}", ex.Message);
}
catch (WirePusherException ex)
{
    logger.LogError("Notification failed - see logs for details");
}
```

### Input Validation

```csharp
// ❌ Bad - No validation
app.MapPost("/notify", async (string title, string message, IWirePusherClient client) =>
{
    await client.SendAsync(title, message);
});

// ✅ Good - Validate input
app.MapPost("/notify", async (string title, string message, IWirePusherClient client) =>
{
    if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
        return Results.BadRequest("Title and message are required");

    if (title.Length > 256 || message.Length > 4096)
        return Results.BadRequest("Content too long");

    try
    {
        var response = await client.SendAsync(title, message);
        return Results.Ok(response);
    }
    catch (WirePusherException)
    {
        return Results.Problem("Failed to send notification");
    }
});
```

### CancellationToken Usage

```csharp
// ❌ Bad - No cancellation support
await client.SendAsync("Title", "Message");

// ✅ Good - Use CancellationToken
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
await client.SendAsync("Title", "Message", cts.Token);

// ✅ Good - ASP.NET Core automatic cancellation
app.MapPost("/notify", async (
    string title,
    string message,
    IWirePusherClient client,
    CancellationToken cancellationToken) =>
{
    await client.SendAsync(title, message, cancellationToken);
});
```

### Encryption Password Security

```csharp
// ❌ Bad - Hardcoded encryption password
var notification = new Notification
{
    Title = "Alert",
    Message = "Sensitive data",
    EncryptionPassword = "password123"
};

// ✅ Good - Environment variable or secret manager
var notification = new Notification
{
    Title = "Alert",
    Message = "Sensitive data",
    EncryptionPassword = Environment.GetEnvironmentVariable("ENCRYPTION_PASSWORD")
};

// ✅ Good - Azure Key Vault (ASP.NET Core)
var notification = new Notification
{
    Title = "Alert",
    Message = "Sensitive data",
    EncryptionPassword = configuration["KeyVault:EncryptionPassword"]
};
```

## Known Security Considerations

### API Token Security

- Tokens are transmitted in API requests and should be kept confidential
- Tokens are stored in memory by the SDK (secure storage is the user's responsibility)
- Compromised tokens can be used to send notifications as your user
- Rotate tokens regularly as a security best practice

### Network Communication

- All communication with WirePusher API is over HTTPS
- The SDK uses .NET's `HttpClient` which respects system-level TLS/SSL settings
- Certificate validation is handled by the .NET runtime
- Minimum TLS 1.2 is enforced by default

### Dependencies

This SDK has **zero external dependencies** to minimize supply chain risks:
- Uses only .NET 6+ built-in libraries (`System.Net.Http`, `System.Text.Json`, `System.Security.Cryptography`)
- No third-party NuGet packages required
- Regular security audits via `dotnet list package --vulnerable`

### Memory Safety

- .NET's memory safety features prevent buffer overflows and memory corruption
- Garbage collection prevents use-after-free vulnerabilities
- Type safety and nullable reference types prevent many common programming errors

## Vulnerability Disclosure Process

When we receive a security bug report:

1. **Confirm the vulnerability** and determine affected versions
2. **Develop and test a fix** for all supported versions
3. **Prepare security advisory** with:
   - Description of the vulnerability
   - Affected versions
   - Fixed versions
   - Workarounds (if any)
   - Credit to reporter
4. **Release patched versions**
5. **Publish security advisory** on GitLab
6. **Notify users** via:
   - GitLab security advisory
   - Project README update
   - NuGet package release notes

## Security Audit History

| Date | Type | Findings | Status |
|------|------|----------|--------|
| TBD  | TBD  | TBD      | TBD    |

## Security Hall of Fame

We thank the following individuals for responsibly disclosing security vulnerabilities:

- (None yet)

## Resources

- [.NET Security Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/security/)
- [OWASP API Security Top 10](https://owasp.org/www-project-api-security/)
- [ASP.NET Core Security](https://learn.microsoft.com/en-us/aspnet/core/security/)

## Questions?

For security-related questions that aren't reporting vulnerabilities:

- Email: security@wirepusher.com
- General questions: support@wirepusher.com

Thank you for helping keep WirePusher and its users safe!
