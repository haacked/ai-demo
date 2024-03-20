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
    OpenAIClientAccessor client,
    FunctionDispatcher dispatcher,
    ILogger<BotMessageConsumer> logger)
    : IConsumer<BotMessageReceived>
{
    // We'll only maintain the last 20 messages in memory.
    //
    // In a *real* app, we'd probably want to use a session-based store so messages are stored specific to a
    // user's session. Not only that, we wouldn't just want to kick out the first messages. We might periodically
    // summarize the existing messages and then kick out the older ones.
    //
    // But for this demo, we'll just use a static queue.
    // TODO: Messages should be partitioned into connection id.
    static readonly LimitedQueue<ChatRequestMessage> Messages = new(20);

    public async Task Consume(ConsumeContext<BotMessageReceived> context)
    {
        var (message, author, connectionId) = context.Message;

        if (message is ".count")
        {
            await SendResponseAsync($"I have {Messages.Count} messages in my history.");
            return;
        }

        if (message is ".clear" or ".clr")
        {
            Messages.Clear();
            await SendResponseAsync($"I have {Messages.Count} messages in my history.");
            return;
        }

        await SendThought("The message addressed me! I'll try and respond.");

        // Prepare the set of messages and options for the GPT call.
        var options = new ChatCompletionsOptions
        {
            Messages =
            {
                new ChatRequestSystemMessage($"You are a helpful assistant who is concise and to the point. You are helping the user {author}."),
            },
        };
        options.Functions.AddRange(dispatcher.GetFunctionDefinitions());
        foreach (var chatMessage in Messages)
        {
            options.Messages.Add(chatMessage);
        }

        // Add the new incoming message.
        var newMessage = new ChatRequestUserMessage(message);
        options.Messages.Add(newMessage);

        // Store the new message in Messages:
        Messages.Enqueue(newMessage);

        try
        {
            var response = await client.GetChatCompletionsAsync(options, context.CancellationToken);
            // It's weird to hard-code the first choice, but we only ask for one choice.
            // It's possible to ask for multiple responses to the same prompt, but I've never needed to do that and
            // even if I did, I wouldn't have any reason to pick anything other than the first one.
            var responseChoice = response.Value.Choices[0];

            // Here's where the magic happens in regards to calling functions.
            int chainedFunctions = 0;
            while (responseChoice.FinishReason == CompletionsFinishReason.FunctionCall &&
                   chainedFunctions < 5) // Don't allow infinite function call loops just. in. case.
            {
                await SendFunction(responseChoice.Message.FunctionCall);

                responseChoice = await CallFunctionAsync(responseChoice);

                if (responseChoice is null)
                {
                    return;
                }

                chainedFunctions++;
            }
            await SendThought("I got a response. It should show up in chat", responseChoice.Message.Content);

            var responseMessage = responseChoice.Message;
            Messages.Enqueue(responseMessage.ToChatRequestMessage());
            await SendResponseAsync(responseMessage.Content);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            logger.ErrorRetrievingChatResponse(ex);
            await SendResponseAsync($"Sorry, I'm having trouble thinking right now: `{ex.Message}`");
        }

        return;

        // Call a GPT function and add the result to the chat history and the completion request options.
        async Task<ChatChoice?> CallFunctionAsync(ChatChoice responseChoice)
        {
            var dispatchResult = await dispatcher.DispatchAsync(
                responseChoice.Message.FunctionCall,
                message,
                context.CancellationToken);
            if (dispatchResult is not null)
            {
                await SendThought("I got a function call result. I'll send it back to GPT to summarize", dispatchResult.Content);

                // We got a function result. Now we send that result *back* to GPT so it can summarize it for the user.
                options.Messages.Add(dispatchResult);

                // And we store this as part of our chat history.
                Messages.Enqueue(dispatchResult);

                var response = await client.GetChatCompletionsAsync(options, context.CancellationToken);
                return response.Value.Choices[0];
            }

            // If we're just storing data, there's nothing to respond with.
            await SendResponseAsync("Ok, got it.");
            return null;
        }

        async Task SendThought(string thought, string? data = null)
            => await hubContext.Clients.Client(connectionId).SendAsync(
                nameof(AssistantHub.BroadcastThought),
                thought,
                data,
                context.CancellationToken);

        async Task SendFunction(FunctionCall functionCall)
            => await hubContext.Clients.Client(connectionId).SendAsync(
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