using AIDemoWeb.Entities.Eventing.Messages;
using Azure.AI.OpenAI;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using OpenAIDemo.Hubs;
using Serious;
using Serious.ChatFunctions;

namespace AIDemoWeb.Entities.Eventing.Consumers;

public class BotMessageConsumer : IConsumer<BotMessageReceived>
{
    readonly IHubContext<BotHub> _hubContext;
    readonly OpenAIClientAccessor _client;
    readonly FunctionDispatcher _dispatcher;
    readonly ILogger<BotMessageConsumer> _logger;

    // We'll only maintain the last 20 messages in memory.
    static readonly LimitedQueue<ChatMessage> Messages = new(20);

    public BotMessageConsumer(
        IHubContext<BotHub> hubContext,
        OpenAIClientAccessor client,
        FunctionDispatcher dispatcher,
        ILogger<BotMessageConsumer> logger)
    {
        _hubContext = hubContext;
        _client = client;
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BotMessageReceived> context)
    {
        var (message, author) = context.Message;

        // Call GPT Directly.
        await SendThought("The message addressed me! I'll try and respond.");
        var options = new ChatCompletionsOptions
        {
            Messages =
            {
                new ChatMessage(ChatRole.System, $"You are a helpful assistant who is witty, pithy, and fun. You are helping the user {author}."),
            },
            Functions = _dispatcher.GetFunctionDefinitions(),
        };
        foreach (var chatMessage in Messages)
        {
            options.Messages.Add(chatMessage);
        }

        // Add the new incoming message.
        var newMessage = new ChatMessage(ChatRole.User, message);
        options.Messages.Add(newMessage);

        // Store the new message in Messages:
        Messages.Enqueue(newMessage);

        try
        {
            var response = await _client.GetChatCompletionsAsync(options, context.CancellationToken);
            var responseChoice = response.Value.Choices[0];

            int chainedFunctions = 0;
            while (responseChoice.FinishReason == CompletionsFinishReason.FunctionCall &&
                   chainedFunctions < 5) // Don't allow infinite loops.
            {
                await SendThought("I have a function that can help with this! I'll call it.");
                await SendFunction(responseChoice.Message.FunctionCall);

                var result = await _dispatcher.DispatchAsync(
                    responseChoice.Message.FunctionCall,
                    message,
                    context.CancellationToken);
                if (result is not null)
                {
                    // TODO: Add a specific result function.
                    await SendThought($"I got a result. I'll send it back to GPT to summarize:\n{result.Content}");

                    // We got a result, which we can send back to GPT so it can summarize it for the user.
                    options.Messages.Add(result);
                    Messages.Enqueue(result);

                    response = await _client.GetChatCompletionsAsync(options, context.CancellationToken);
                    responseChoice = response.Value.Choices[0];
                    await SendThought($"I got a summarized response. It should show up in chat:\n{result.Content}");

                }
                else
                {
                    // If we're just storing data, there's nothing to respond with.
                    await SendResponseAsync("Ok, got it.");
                    return;
                }

                chainedFunctions++;
            }

            var responseMessage = responseChoice.Message;
            Messages.Enqueue(responseMessage);
            await SendResponseAsync(responseMessage.Content);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            _logger.ErrorRetrievingChatResponse(ex);
            await SendResponseAsync($"Sorry, I'm having trouble thinking right now: `{ex.Message}`");
        }

        return;

        async Task SendThought(string thought)
            => await _hubContext.Clients.All.SendAsync(
                nameof(BotHub.BroadcastThought),
                thought,
                context.CancellationToken);

        async Task SendFunction(FunctionCall functionCall)
            => await _hubContext.Clients.All.SendAsync(
                nameof(BotHub.BroadcastFunctionCall),
                functionCall.Name,
                functionCall.Arguments,
                context.CancellationToken);

        async Task SendResponseAsync(string response)
        {
            await _hubContext.Clients.All.SendAsync(
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