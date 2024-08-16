using AIDemo.Web.Messages;
using AIDemo.Library.Clients;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using AIDemo.Hubs;
using StackExchange.Redis;

namespace AIDemoWeb.Entities.Eventing.Consumers;

public class BotMessageConsumer(
    IHubContext<BotHub> hubContext,
    Kernel kernel,
    IConnectionMultiplexer connectionMultiplexer) : IConsumer<BotMessageReceived>
{
    public async Task Consume(ConsumeContext<BotMessageReceived> context)
    {
        // TODO: We probably want to provide the author to the system prompt.
        var (message, author, connectionId, userIdentifier) = context.Message;

        var cache = new ChatHistoryCache(
            connectionMultiplexer,
            systemPrompt: $"You are a helpful assistant who is concise and to the point. You are in a conversation with {author} with the identifier {userIdentifier}.",
            userIdentifier ?? Guid.NewGuid().ToString());
        var history = await cache.GetChatHistoryAsync();

        await SendThought("The message addressed me! I'll try and respond.");

        history.AddUserMessage(message);

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        try
        {
            // Get the response from the AI
            var result = await chatCompletionService.GetChatMessageContentAsync(
                history,
                kernel: kernel);

            // Add the message from the agent to the chat history
            history.AddMessage(result.Role, result.Content ?? string.Empty);

            await SendThought(
                "I got a response. It should show up in chat",
                $"{result.Role}: {result.Content}");

            if (result.Content is not (null or []))
            {
                await SendResponseAsync(result.Content, ChatMessageRole.Assistant);
            }

            await cache.SaveChatHistoryAsync(history);
        }
        catch (HttpOperationException e)
        {
            await SendResponseAsync(e.ToString(), ChatMessageRole.Assistant);
        }

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