using AIDemoWeb.Entities.Eventing.Messages;
using Azure.AI.OpenAI;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using OpenAIDemo.Hubs;
using Serious;
using Serious.ChatFunctions;

namespace AIDemoWeb.Entities.Eventing.Consumers;

public class ChatMessageConsumer : IConsumer<ChatMessageReceived>
{
    readonly IHubContext<ChatHub> _hubContext;
    readonly OpenAIClientAccessor _client;
    readonly FunctionDispatcher _dispatcher;

    // We'll only maintain the last 20 messages in memory.
    static readonly LimitedQueue<ChatMessage> Messages = new(20);

    public ChatMessageConsumer(IHubContext<ChatHub> hubContext, OpenAIClientAccessor client, FunctionDispatcher dispatcher)
    {
        _hubContext = hubContext;
        _client = client;
        _dispatcher = dispatcher;
    }

    public async Task Consume(ConsumeContext<ChatMessageReceived> context)
    {
        var message = context.Message;

        Messages.Enqueue(new ChatMessage(ChatRole.User, $"{message.Author}:{message.Message}"));

        if (message.Message.StartsWith("Hey bot", StringComparison.OrdinalIgnoreCase))
        {
            var options = new ChatCompletionsOptions
            {
                Messages =
                {
                    new ChatMessage(ChatRole.System, "You are observing a conversation in a chat room with multiple participants. Each message starts with the participant's name which must not be altered. If you can be of assistance, please do chime in."),
                },
                Functions = _dispatcher.GetFunctionDefinitions(),
            };
            foreach (var chatMessage in Messages)
            {
                options.Messages.Add(chatMessage);
            }
            var response = await _client.GetChatCompletionsAsync(options, context.CancellationToken);
            var responseChoice = response.Value.Choices[0];
            if (responseChoice.FinishReason == CompletionsFinishReason.FunctionCall)
            {
                var result = await _dispatcher.DispatchAsync(
                    responseChoice.Message.FunctionCall,
                    context.CancellationToken);

                if (result is not null)
                {
                    // We got a result, which we can send back to GPT so it can summarize it for the user.
                    options.Messages.Add(result);
                    Messages.Enqueue(result);

                    response = await _client.GetChatCompletionsAsync(options, context.CancellationToken);
                    responseChoice = response.Value.Choices[0];
                }
            }
            var answer = responseChoice.Message.Content;
            await _hubContext.Clients.All.SendAsync("messageReceived", "The Bot", answer,
                context.CancellationToken);
        }
    }
}