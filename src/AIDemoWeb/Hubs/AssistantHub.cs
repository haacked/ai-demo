using AIDemoWeb.Entities.Eventing.Messages;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace OpenAIDemo.Hubs;

public class AssistantHub : Hub
{
    readonly IPublishEndpoint _publishEndpoint;

    public AssistantHub(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Broadcast(string message, bool isUser, string assistantName, string assistantId, string threadId)
    {
        await Clients.All.SendAsync("Broadcast", message, isUser, assistantName, assistantId, threadId);
        await _publishEndpoint.Publish(new AssistantMessageReceived(message, assistantName, assistantId, threadId));
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