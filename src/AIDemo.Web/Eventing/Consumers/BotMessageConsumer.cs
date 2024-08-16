using AIDemo.Hubs;
using AIDemo.Web.Messages;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AIDemoWeb.Entities.Eventing.Consumers;

public class BotMessageConsumer(
    IHubContext<BotHub> hubContext,
    Kernel kernel) : IConsumer<BotMessageReceived>
{
    public async Task Consume(ConsumeContext<BotMessageReceived> context)
    {
        // TODO: We probably want to provide the author to the system prompt.
        var (message, author, connectionId, userIdentifier) = context.Message;

        await SendThought("The message addressed me! I'll try and respond.");


        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // Get the response from the AI
        var result = await chatCompletionService.GetChatMessageContentAsync(
            message,
            kernel: kernel);

        await SendThought(
            "I got a response. It should show up in chat",
            $"{result.Role}: {result.Content}");

        if (result.Content is not (null or []))
        {
            await SendResponseAsync(result.Content, ChatMessageRole.Assistant);
        }

        return;

        async Task SendThought(string thought, string? data = null)
            => await hubContext.Clients.Client(connectionId).SendAsync(
                nameof(AssistantHub.BroadcastThought),
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