using AIDemoWeb.Entities.Eventing.Messages;
using Azure.AI.OpenAI;
using Haack.AIDemoWeb.Library;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using OpenAIDemo.Hubs;
using Serious;
using Serious.ChatFunctions;

namespace AIDemoWeb.Entities.Eventing.Consumers;

public class BotMessageConsumer(
    IHubContext<BotHub> hubContext,
#pragma warning disable CS9113 // Parameter is unread.
    OpenAIClientAccessor client,
    //FunctionDispatcher dispatcher,
    ILogger<BotMessageConsumer> logger)
    : IConsumer<BotMessageReceived>
{
    public async Task Consume(ConsumeContext<BotMessageReceived> context)
    {
        var (message, author, connectionId) = context.Message;

        await SendThought("The message addressed me! I'll try and respond.");

        // This is where we need to sprinkle some AI on it.
        await SendResponseAsync("\ud83d\udca9");

        return;

        async Task SendThought(string thought, string? data = null)
            => await hubContext.Clients.Client(connectionId).SendAsync(
                nameof(AssistantHub.BroadcastThought),
                thought,
                data,
                context.CancellationToken);

#pragma warning disable CS8321 // Local function is declared but never used
        async Task SendFunction(FunctionCall functionCall)
            => await hubContext.Clients.Client(connectionId!).SendAsync(
                nameof(BotHub.BroadcastFunctionCall),
                functionCall.Name,
                functionCall.Arguments,
                context.CancellationToken);

        async Task SendResponseAsync(string response)
        {
            await hubContext.Clients.Client(connectionId).SendAsync(
                nameof(BotHub.Broadcast),
                response,
                "Clippy", // author
                false, // isUser
                context.CancellationToken);
        }
    }
}

public static partial class BotMessageConsumerLoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Error retrieving Chat GPT response")]
    public static partial void ErrorRetrievingChatResponse(
        this ILogger<BotMessageConsumer> logger,
        Exception exception);
}