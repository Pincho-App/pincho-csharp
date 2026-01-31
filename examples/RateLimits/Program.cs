using Pincho;

var token = Environment.GetEnvironmentVariable("WIREPUSHER_TOKEN") ?? "your_token_here";
var client = new PinchoClient(token);

Console.WriteLine("Rate Limit Monitoring Example\n");
Console.WriteLine("Note: Rate limit information is returned in HTTP headers.");
Console.WriteLine("This example demonstrates sending notifications and monitoring responses.\n");

// Send multiple notifications to observe rate limit behavior
Console.WriteLine("--- Sending multiple requests ---\n");
for (int i = 1; i <= 3; i++)
{
    Console.WriteLine($"Request {i}:");
    try
    {
        var response = await client.SendAsync($"Test {i}", $"Request number {i} to monitor rate limits");
        Console.WriteLine($"  Status: {response.Status}");
        Console.WriteLine($"  Message: {response.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Error: {ex.Message}");
    }
    Console.WriteLine();

    // Small delay between requests
    await Task.Delay(500);
}

Console.WriteLine("Rate limit monitoring complete.\n");
Console.WriteLine("Notes:");
Console.WriteLine("- Rate limits are automatically enforced by the server");
Console.WriteLine("- RateLimitException is automatically retried with exponential backoff");
Console.WriteLine("- The Retry-After header is respected for optimal retry timing");
Console.WriteLine("- Auth errors (401/403) are not retried");
Console.WriteLine("- Validation errors (400) are not retried");
Console.WriteLine("- Server errors (5xx) are retried with backoff");
