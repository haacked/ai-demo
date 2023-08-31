using AIDemoWeb.Entities.Eventing.Messages;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using OpenAIDemo.Hubs;

namespace AIDemoWeb.Entities.Eventing.Consumers;

public class ChatMessageConsumer : IConsumer<ChatMessage>
{
    readonly IHubContext<ChatHub> _hubContext;

    public ChatMessageConsumer(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Consume(ConsumeContext<ChatMessage> context)
    {
        var message = context.Message;

        if (message.Message.StartsWith("Hey bot", StringComparison.OrdinalIgnoreCase))
        {
            await _hubContext.Clients.All.SendAsync("messageReceived", "The Bot", "What can I do for you?");
        }
    }
}