using AIDemoWeb.Entities.Eventing.Messages;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAIDemo.Hubs;

namespace AIDemoWeb.Entities.Eventing.Consumers;

public class BotMessageConsumer(IHubContext<BotHub> hubContext, Kernel kernel) : IConsumer<BotMessageReceived>
{
    // We'll only maintain the last 20 messages in memory.
    //
    // In a *real* app, we'd probably want to use a session-based store so messages are stored specific to a
    // user's session. Not only that, we wouldn't just want to kick out the first messages. We might periodically
    // summarize the existing messages and then kick out the older ones.
    //
    // But for this demo, we'll just use a static queue.
    // TODO: Messages should be partitioned into connection id.
    static readonly ChatHistory ChatHistory = new()
    {
        new ChatMessageContent(AuthorRole.System, "You are a helpful assistant who is concise and to the point.")
    };

    public async Task Consume(ConsumeContext<BotMessageReceived> context)
    {
        // TODO: We probably want to provide the author to the system prompt.
        var (message, _, connectionId) = context.Message;

        if (message is ".count")
        {
            await SendResponseAsync($"I have {ChatHistory.Count} messages in my history.");
            return;
        }

        if (message is ".clear" or ".clr")
        {
            ChatHistory.Clear();
            await SendResponseAsync($"I have {ChatHistory.Count} messages in my history.");
            return;
        }

        await SendThought("The message addressed me! I'll try and respond.");

        ChatHistory.AddUserMessage(message);

        // Enable auto function calling
        OpenAIPromptExecutionSettings openAiPromptExecutionSettings = new()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // Get the response from the AI
        var result = await chatCompletionService.GetChatMessageContentAsync(
            ChatHistory,
            executionSettings: openAiPromptExecutionSettings,
            kernel: kernel);

        // Add the message from the agent to the chat history
        ChatHistory.AddMessage(result.Role, result.Content ?? string.Empty);

        await SendThought(
            "I got a response. It should show up in chat",
            $"{result.Role}: {result.Content}");

        if (result.Content is not null)
        {
            await SendResponseAsync(result.Content);
        }

        return;

        async Task SendThought(string thought, string? data = null)
            => await hubContext.Clients.Client(connectionId).SendAsync(
                nameof(AssistantHub.BroadcastThought),
                thought,
                data,
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