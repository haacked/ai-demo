using AIDemoWeb.Entities.Eventing.Messages;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace OpenAIDemo.Hubs;

public class ChatHub : Hub
{
    readonly IPublishEndpoint _publishEndpoint;

    public ChatHub(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    /// <summary>
    /// When a new message is received, broadcast it to all clients.
    /// </summary>
    /// <param name="username">The name of the person sending the message.</param>
    /// <param name="message">The message text.</param>
    public async Task NewMessage(string username, string message)
    {
        await Clients.All.SendAsync("messageReceived", username, message);
        await _publishEndpoint.Publish(new ChatMessage { Author = username, Message = message });
    }
}