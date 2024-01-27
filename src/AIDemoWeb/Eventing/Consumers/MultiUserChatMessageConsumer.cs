using AIDemoWeb.Entities.Eventing.Messages;
using Azure.AI.OpenAI;
using Haack.AIDemoWeb.Library;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using OpenAIDemo.Hubs;
using Serious;
using Serious.ChatFunctions;

namespace AIDemoWeb.Entities.Eventing.Consumers;

/// <summary>
/// This consumer is used for the multi-user chat demo which is legacy
/// and not one I use in talks.
/// </summary>
public class MultiUserChatMessageConsumer : IConsumer<MultiUserChatMessageReceived>
{
    readonly IHubContext<MultiUserChatHub> _hubContext;
    readonly OpenAIClientAccessor _client;
    readonly FunctionDispatcher _dispatcher;

    // We'll only maintain the last 20 messages in memory.
    static readonly LimitedQueue<ChatMessage> Messages = new(20);

    public MultiUserChatMessageConsumer(
        IHubContext<MultiUserChatHub> hubContext,
        OpenAIClientAccessor client,
        FunctionDispatcher dispatcher)
    {
        _hubContext = hubContext;
        _client = client;
        _dispatcher = dispatcher;
    }

    public async Task Consume(ConsumeContext<MultiUserChatMessageReceived> context)
    {
        var (author, message) = context.Message;

        Messages.Enqueue(new ChatMessage(ChatRole.User, $"{author}:{message}"));

        if (message.StartsWith("Hey bot", StringComparison.OrdinalIgnoreCase))
        {
            await SendThought("The message addressed me! I'll try and respond.");
            var options = new ChatCompletionsOptions
            {
                Messages =
                {
                    new ChatMessage(ChatRole.System, "You are observing a conversation in a chat room with multiple participants. Each message starts with the participant's name which must not be altered. If you can be of assistance, please do chime in, otherwise stay quiet and let them speak."),
                },
                Functions = _dispatcher.GetFunctionDefinitions(),
            };
            foreach (var chatMessage in Messages)
            {
                options.Messages.Add(chatMessage);
            }

            var response = await _client.GetChatCompletionsAsync(options, context.CancellationToken);
            var responseChoice = response.Value.Choices[0];

            int chainedFunctions = 0;
            while (responseChoice.FinishReason == CompletionsFinishReason.FunctionCall && chainedFunctions < 5) // Don't allow infinite loops.
            {
                await SendThought("I have a function that can help with this! I'll call it.");
                await SendFunction(responseChoice.Message.FunctionCall);

                var result = await _dispatcher.DispatchAsync(
                    responseChoice.Message.FunctionCall,
                    message,
                    context.CancellationToken);
                if (result is not null)
                {
                    await SendThought($"I got a result. I'll send it back to GPT to summarize", result.Content);

                    // We got a result, which we can send back to GPT so it can summarize it for the user.
                    options.Messages.Add(result);
                    Messages.Enqueue(result);

                    response = await _client.GetChatCompletionsAsync(options, context.CancellationToken);
                    responseChoice = response.Value.Choices[0];
                }
                else
                {
                    // If we're just storing data, there's nothing to respond with.
                    await SendResponseAsync(context, "Ok, got it.");
                    return;
                }

                chainedFunctions++;
            }

            await SendResponseAsync(context, responseChoice.Message.Content);
        }
        else
        {
            await SendThought("The message didn't address me (by starting with \"Hey bot\") so I'll ignore it.");
        }

        return;

        async Task SendThought(string thought, string? data = null)
        {
            var thoughtMessage = thought;
            if (data is not null && data.StartsWith("\"{", StringComparison.Ordinal) &&
                                data.EndsWith("}\"", StringComparison.Ordinal))
            {
                var cleanData = data[1..^1].Replace(@"\u0022", "\"", StringComparison.Ordinal).JsonPrettify();
                thoughtMessage = thought + ":\n" + cleanData;
            }

            await _hubContext.Clients.All.SendAsync(
                nameof(AssistantHub.BroadcastThought),
                thoughtMessage,
                context.CancellationToken);
        }

        async Task SendFunction(FunctionCall functionCall)
            => await _hubContext.Clients.All.SendAsync(
                nameof(AssistantHub.BroadcastFunctionCall),
                functionCall.Name,
                functionCall.Arguments,
                context.CancellationToken);
    }

    async Task SendResponseAsync(ConsumeContext<MultiUserChatMessageReceived> context, string response)
    {
        await _hubContext.Clients.All.SendAsync(
            "messageReceived",
            "The Bot",
            response,
            context.CancellationToken);
        Messages.Enqueue(new ChatMessage(ChatRole.Assistant, response));
    }
}