using System.Text.Json;
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
    //FunctionDispatcher dispatcher,
    ILogger<BotMessageConsumer> logger)
    : IConsumer<BotMessageReceived>
{
    public async Task Consume(ConsumeContext<BotMessageReceived> context)
    {
        var (message, author, connectionId) = context.Message;

        await SendThought("The message addressed me! I'll try and respond.");

        // Prepare the set of messages and options for the GPT call.
        var options = new ChatCompletionsOptions
        {
            Messages =
            {
                new ChatRequestSystemMessage($"You are a helpful assistant who is concise and to the point. You are helping the user {author}."),
                new ChatRequestUserMessage(message),
            },
        };

        try
        {
            var response = await client.GetChatCompletionsAsync(options, context.CancellationToken);
            // It's weird to hard-code the first choice, but we only ask for one choice.
            // It's possible to ask for multiple responses to the same prompt, but I've never needed to do that and
            // even if I did, I wouldn't have any reason to pick anything other than the first one.
            var responseChoice = response.Value.Choices[0];

            await SendThought($"I got a response. It should show up in chat", responseChoice.Message.Content);

            var responseMessage = responseChoice.Message;
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