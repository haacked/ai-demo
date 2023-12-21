using AIDemoWeb.Entities.Eventing.Messages;
using Azure.AI.OpenAI;
using Haack.AIDemoWeb.Library;
using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using OpenAIDemo.Hubs;
using Serious;
using Serious.ChatFunctions;

namespace AIDemoWeb.Entities.Eventing.Consumers;

public class AssistantMessageConsumer : IConsumer<AssistantMessageReceived>
{
    readonly IHubContext<AssistantHub> _hubContext;
    readonly OpenAIClientAccessor _client;
    readonly FunctionDispatcher _dispatcher;
    readonly IOpenAIClient _openAIClient;
    readonly OpenAIOptions _options;

    // We'll only maintain the last 20 messages in memory.
    static readonly LimitedQueue<ChatMessage> Messages = new(20);

    public AssistantMessageConsumer(
        IHubContext<AssistantHub> hubContext,
        OpenAIClientAccessor client,
        FunctionDispatcher dispatcher,
        IOpenAIClient openAIClient,
        IOptions<OpenAIOptions> options)
    {
        _hubContext = hubContext;
        _client = client;
        _dispatcher = dispatcher;
        _openAIClient = openAIClient;
        _options = options.Value;
    }

    public async Task Consume(ConsumeContext<AssistantMessageReceived> context)
    {
        var (message, assistantName, assistantId, threadId) = context.Message;

        if (assistantId is null || threadId is null)
        {
            // Call GPT Directly.
            await SendThought("The message addressed me! I'll try and respond.");
            var options = new ChatCompletionsOptions
            {
                Messages =
                {
                    new ChatMessage(ChatRole.System, "You are a helpful assistant who is witty, pithy, and fun."),
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
                await SendResponseAsync($"Sorry, I'm having trouble thinking right now: `{ex.Message}`");
            }
        }
        else
        {
            // Create a new message in the thread
            var newMessage = await _openAIClient.CreateMessageAsync(
                _options.ApiKey.Require(),
                threadId.Require(),
                new MessageCreateBody { Content = message },
                context.CancellationToken);

            var runCreateDate = DateTime.UtcNow;

            // Create a run for the thread.
            var run = await _openAIClient.CreateRunAsync(
                _options.ApiKey.Require(),
                threadId,
                new ThreadRunCreateBody
                {
                    AssistantId = assistantId.Require()
                },
                context.CancellationToken);

            var retryAttempt = 1;

            // Now we need to poll the run for the bot's response. We'll do it up to a minute.
            while (run.Status is not ("completed" or "cancelled" or "failed" or "expired") &&
                   DateTime.UtcNow < runCreateDate.AddMinutes(1))
            {
                if (retryAttempt is 4)
                {
                    // We only want to send this message once and only if the bot is taking a while to respond.
                    await SendResponseAsync("Thinking…");
                }

                await Task.Delay(1000, context.CancellationToken);
                run = await _openAIClient.GetRunAsync(
                    _options.ApiKey.Require(),
                    run.Id,
                    threadId,
                    context.CancellationToken);
                retryAttempt++;
            }

            if (run.Status is "completed")
            {
                // Grab the messages added by the assistant.
                var response = await _openAIClient.GetMessagesAsync(
                    _options.ApiKey.Require(),
                    threadId,
                    order: "asc",
                    after: newMessage.Id,
                    cancellationToken: context.CancellationToken);

                foreach (var reply in response.Data.SelectMany(m => m.ToBlazorMessages()).Where(m => !m.IsUser))
                {
                    await SendResponseAsync(reply.Text, reply.Annotations);
                }
            }
            else
            {
                await SendResponseAsync($"Sorry, I'm having trouble thinking right now: `{run.Status}`");
            }
        }

        return;

        async Task SendThought(string thought)
            => await _hubContext.Clients.All.SendAsync(
                nameof(AssistantHub.BroadcastThought),
                thought,
                context.CancellationToken);

        async Task SendFunction(FunctionCall functionCall)
            => await _hubContext.Clients.All.SendAsync(
                nameof(AssistantHub.BroadcastFunctionCall),
                functionCall.Name,
                functionCall.Arguments,
                context.CancellationToken);

        async Task SendResponseAsync(string response, IReadOnlyList<Annotation>? annotations = null)
        {
            await _hubContext.Clients.All.SendAsync(
                nameof(AssistantHub.Broadcast),
                response,
                false, // isUser
                assistantName,
                assistantId,
                threadId,
                annotations ?? Array.Empty<Annotation>(),
                context.CancellationToken);
        }
    }
}