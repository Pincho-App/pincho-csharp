using WirePusher;
using WirePusher.Exceptions;

var token = Environment.GetEnvironmentVariable("WIREPUSHER_TOKEN") ?? "your_token_here";
var client = new WirePusherClient(token);

Console.WriteLine("Demonstrating error handling patterns...\n");

// Test 1: Successful send
Console.WriteLine("1. Sending valid notification...");
try
{
    var response = await client.SendAsync("Test", "Valid notification");
    Console.WriteLine($"   Success: {response.Status}");
}
catch (WirePusherException ex)
{
    Console.WriteLine($"   Error: {ex.Message}");
}

// Test 2: Using invalid token (simulated with different client)
Console.WriteLine("\n2. Testing with invalid token...");
try
{
    var badClient = new WirePusherClient("invalid_token");
    await badClient.SendAsync("Test", "This will fail");
}
catch (AuthenticationException ex)
{
    Console.WriteLine($"   AuthenticationException caught (expected)");
    Console.WriteLine($"   Message: {ex.Message}");
    Console.WriteLine($"   Is Retryable: {ex.IsRetryable}"); // false
}
catch (Exception ex)
{
    Console.WriteLine($"   {ex.GetType().Name}: {ex.Message}");
}

// Test 3: Comprehensive error handling pattern
Console.WriteLine("\n3. Comprehensive error handling pattern:");
try
{
    await client.SendAsync("Title", "Message");
    Console.WriteLine("   Success!");
}
catch (AuthenticationException ex)
{
    // Invalid token (401/403) - not retried
    Console.WriteLine($"   Auth failed: {ex.Message}");
}
catch (ValidationException ex)
{
    // Invalid parameters (400) - not retried
    Console.WriteLine($"   Validation error: {ex.Message}");
}
catch (RateLimitException ex)
{
    // Rate limited (429) - automatically retried with backoff
    Console.WriteLine($"   Rate limited: {ex.Message}");
}
catch (ServerException ex)
{
    // Server error (5xx) - automatically retried
    Console.WriteLine($"   Server error: {ex.Message}");
}
catch (NetworkException ex)
{
    // Network/timeout error - automatically retried
    Console.WriteLine($"   Network error: {ex.Message}");
}

// Test 4: Check IsRetryable property
Console.WriteLine("\n4. Error types and retryability:");
Console.WriteLine("   AuthenticationException - IsRetryable: false");
Console.WriteLine("   ValidationException - IsRetryable: false");
Console.WriteLine("   RateLimitException - IsRetryable: true");
Console.WriteLine("   ServerException - IsRetryable: true");
Console.WriteLine("   NetworkException - IsRetryable: true");

Console.WriteLine("\nError handling demonstration complete.");
