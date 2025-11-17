using WirePusher;

var token = Environment.GetEnvironmentVariable("WIREPUSHER_TOKEN") ?? "your_token_here";
var client = new WirePusherClient(token);

// Send free-form text, AI generates structured notification
var text = "deployment finished successfully, v2.1.3 is live on production servers";

Console.WriteLine("Sending NotifAI request...");
Console.WriteLine($"Input text: {text}");

var response = await client.NotifAIAsync(text);
Console.WriteLine("\nAI-generated notification:");
Console.WriteLine($"Status: {response.Status}");
Console.WriteLine($"Message: {response.Message}");

// With type override
Console.WriteLine("\n--- With type override ---");
var request = new NotifAIRequest
{
    Text = "cpu at 95% on web-server-3, memory usage critical",
    Type = "alert"
};

response = await client.NotifAIAsync(request);
Console.WriteLine($"Input text: {request.Text}");
Console.WriteLine($"Type override: {request.Type}");
Console.WriteLine($"\nStatus: {response.Status}");
Console.WriteLine($"Message: {response.Message}");
