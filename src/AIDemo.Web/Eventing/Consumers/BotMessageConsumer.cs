using AIDemoWeb.Entities.Eventing.Messages;
using Haack.AIDemoWeb.Library;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAIDemo.Hubs;
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
        var (message, _, connectionId, userIdentifier) = context.Message;

        var cache = new ChatHistoryCache(
            connectionMultiplexer,
            "You are a helpful assistant who is concise and to the point.",
            userIdentifier ?? Guid.NewGuid().ToString());
        var history = await cache.GetChatHistoryAsync();

        if (message is ".count")
        {
            await SendResponseAsync($"I have {history.Count - 1} messages in my history.", AuthorRole.Assistant);
            return;
        }

        if (message is ".replay" or ".history")
        {
            foreach (var msg in history.Skip(1)
                         .Where(m => m.Content is not null or [])
                         .Where(m => m.Role == AuthorRole.User || m.Role == AuthorRole.Assistant)) // Skip the system prompt
            {
                await SendResponseAsync(msg.Content ?? string.Empty, msg.Role);
            }
            return;
        }

        if (message is ".clear" or ".clr")
        {
            await cache.DeleteChatHistoryAsync();
            await SendResponseAsync("I have 0 messages in my history.", AuthorRole.Assistant);
            return;
        }

        await SendThought("The message addressed me! I'll try and respond.");

        history.AddUserMessage(message);

        // Enable auto function calling
        OpenAIPromptExecutionSettings openAiPromptExecutionSettings = new()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // Get the response from the AI
        var result = await chatCompletionService.GetChatMessageContentAsync(
            history,
            executionSettings: openAiPromptExecutionSettings,
            kernel: kernel);

        // Add the message from the agent to the chat history
        history.AddMessage(result.Role, result.Content ?? string.Empty);

        await SendThought(
            "I got a response. It should show up in chat",
            $"{result.Role}: {result.Content}");

        if (result.Content is not null)
        {
            await SendResponseAsync(result.Content, AuthorRole.Assistant);
        }

        await cache.SaveChatHistoryAsync(history);

        return;

        async Task SendThought(string thought, string? data = null)
            => await hubContext.Clients.Client(connectionId).SendAsync(
                nameof(AssistantHub.BroadcastThought),
                thought,
                data,
                context.CancellationToken);

        async Task SendResponseAsync(string response, AuthorRole authorRole)
        {
            await hubContext.Clients.Client(connectionId).SendAsync(
                nameof(BotHub.Broadcast),
                response,
                "Clippy", // author
                authorRole,
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