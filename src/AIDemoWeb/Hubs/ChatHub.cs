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
    /// When a new message is received, broadcast it to all clients and then send it to the AI to respond if
    /// necessary.
    /// </summary>
    /// <param name="username">The name of the person sending the message.</param>
    /// <param name="message">The message text.</param>
    public async Task NewMessage(string username, string message)
    {
        await Clients.All.SendAsync("messageReceived", username, message);
        await _publishEndpoint.Publish(new ChatMessageReceived { Author = username, Message = message });
    }

    /// <summary>
    /// When the AI has thoughts about what it is doing, broadcast it to all clients.
    /// </summary>
    /// <param name="message">The thought.</param>
    public async Task NewThought(string message)
    {
        await Clients.All.SendAsync("thoughtReceived", message);
    }
}