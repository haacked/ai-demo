using AIDemoWeb.Entities.Eventing.Messages;
using Azure.AI.OpenAI;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using OpenAIDemo.Hubs;
using Serious;

namespace AIDemoWeb.Entities.Eventing.Consumers;

public class ChatMessageConsumer : IConsumer<ChatMessageReceived>
{
    readonly IHubContext<ChatHub> _hubContext;
    readonly OpenAIClientAccessor _client;

    // We'll only maintain the last 20 messages in memory.
    readonly LimitedQueue<ChatMessageReceived> _messages = new(20);

    public ChatMessageConsumer(IHubContext<ChatHub> hubContext, OpenAIClientAccessor client)
    {
        _hubContext = hubContext;
        _client = client;
    }

    public async Task Consume(ConsumeContext<ChatMessageReceived> context)
    {
        var message = context.Message;

        _messages.Enqueue(message);

        if (message.Message.StartsWith("Hey bot", StringComparison.OrdinalIgnoreCase))
        {
            var options = new ChatCompletionsOptions
            {
                Messages =
                {
                    new ChatMessage(ChatRole.System,  "You are observing a conversation in a chat room. If you can be of assistance, please do chime in."),
                    new ChatMessage(ChatRole.User, message.Message),
                }
            };
            var response = await _client.GetChatCompletionsAsync(options, context.CancellationToken);
            var answer = string.Join("\n", response.Value.Choices.Select(c => c.Message.Content));
            await _hubContext.Clients.All.SendAsync("messageReceived", "The Bot", answer, context.CancellationToken);
        }
    }
}