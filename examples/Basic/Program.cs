using WirePusher;

// Get token from environment variable or use default for testing
var token = Environment.GetEnvironmentVariable("WIREPUSHER_TOKEN") ?? "your_token_here";

var client = new WirePusherClient(token);

// Send a simple notification
Console.WriteLine("Sending basic notification...");
var response = await client.SendAsync("Hello from C#", "This is a test notification from the WirePusher C# library.");
Console.WriteLine($"Status: {response.Status}");
Console.WriteLine($"Message: {response.Message}");

// Send another notification
Console.WriteLine("\nSending deploy notification...");
response = await client.SendAsync("Deploy Complete", "Version 1.2.3 deployed to production");
Console.WriteLine($"Status: {response.Status}");
Console.WriteLine($"Message: {response.Message}");
