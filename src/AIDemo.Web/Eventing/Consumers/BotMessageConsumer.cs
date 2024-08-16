using AIDemo.Hubs;
using AIDemo.Web.Messages;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace AIDemoWeb.Entities.Eventing.Consumers;

public class BotMessageConsumer(IHubContext<BotHub> hubContext) : IConsumer<BotMessageReceived>
{
    public async Task Consume(ConsumeContext<BotMessageReceived> context)
    {
        // TODO: We probably want to provide the author to the system prompt.
        var (message, author, connectionId, userIdentifier) = context.Message;

        if (message.StartsWith('.'))
        {
            // Ignore commands for now.
            return;
        }

        await SendThought("The message addressed me! I'll try and respond.");

        // TODO: Sprinkle some of that AI magic here.
        await SendResponseAsync("\ud83d\udca9", ChatMessageRole.Assistant);

        return;

        async Task SendThought(string thought, string? data = null)
            => await hubContext.Clients.Client(connectionId).SendAsync(
                nameof(BotHub.BroadcastThought),
                thought,
                data,
                context.CancellationToken);

        async Task SendResponseAsync(string response, ChatMessageRole messageRole)
        {
            await hubContext.Clients.Client(connectionId).SendAsync(
                nameof(BotHub.Broadcast),
                response,
                "Clippy", // author
                messageRole,
                userIdentifier,
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