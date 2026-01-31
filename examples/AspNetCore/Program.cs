using Pincho;

var builder = WebApplication.CreateBuilder(args);

// Register WirePusher client as singleton
// IPinchoClient interface enables dependency injection and testing
builder.Services.AddSingleton<IPinchoClient>(sp =>
{
    var token = builder.Configuration["Pincho:Token"]
        ?? Environment.GetEnvironmentVariable("WIREPUSHER_TOKEN")
        ?? "your_token_here";
    return new PinchoClient(token);
});

var app = builder.Build();

// Simple endpoint to send notifications
app.MapPost("/notify", async (IPinchoClient client, NotifyRequest request) =>
{
    var response = await client.SendAsync(request.Title, request.Message ?? "");
    return Results.Ok(new { response.Status, response.Message });
});

// Endpoint with full notification options
app.MapPost("/notify/advanced", async (IPinchoClient client, Notification notification) =>
{
    var response = await client.SendNotificationAsync(notification);
    return Results.Ok(new { response.Status, response.Message });
});

// NotifAI endpoint
app.MapPost("/notify/ai", async (IPinchoClient client, string text) =>
{
    var response = await client.NotifAIAsync(text);
    return Results.Ok(new { response.Status, response.Message });
});

// Health check
app.MapGet("/", () => "WirePusher ASP.NET Core Example");

Console.WriteLine("ASP.NET Core WirePusher Example");
Console.WriteLine("Endpoints:");
Console.WriteLine("  POST /notify - Send basic notification");
Console.WriteLine("  POST /notify/advanced - Send with all options");
Console.WriteLine("  POST /notify/ai - Send NotifAI request");
Console.WriteLine("  GET / - Health check");
Console.WriteLine("\nStarting server on http://localhost:5000...");

app.Run("http://localhost:5000");

// Request DTOs
public record NotifyRequest(string Title, string? Message);
