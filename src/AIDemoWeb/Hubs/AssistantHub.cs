using Microsoft.AspNetCore.SignalR;

namespace OpenAIDemo.Hubs;

public class AssistantHub : Hub
{
    public const string HubUrl = "/assistant-hub";

    public async Task Broadcast(string username, string message)
    {
        await Clients.All.SendAsync("Broadcast", username, message);
    }

    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"{Context.ConnectionId} connected");
        return base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"Disconnected {exception?.Message} {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }
}