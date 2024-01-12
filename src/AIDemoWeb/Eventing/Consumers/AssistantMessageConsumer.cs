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
    readonly FunctionDispatcher _dispatcher;
    readonly IOpenAIClient _openAIClient;
    readonly OpenAIOptions _options;

    public AssistantMessageConsumer(
        IHubContext<AssistantHub> hubContext,
        FunctionDispatcher dispatcher,
        IOpenAIClient openAIClient,
        IOptions<OpenAIOptions> options)
    {
        _hubContext = hubContext;
        _dispatcher = dispatcher;
        _openAIClient = openAIClient;
        _options = options.Value;
    }

    public async Task Consume(ConsumeContext<AssistantMessageReceived> context)
    {
        var (message, assistantName, assistantId, threadId) = context.Message;

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


            if (run.Status is "requires_action")
            {
                await SendThought($"Assistant run {run.Id} needs me to call a function.");

                var outputs = new List<ToolOutput>();
                foreach (var toolCall in run.RequiredAction?.SubmitToolOutputs.ToolCalls ?? Enumerable.Empty<RequiredToolCall>())
                {
                    await SendFunction(toolCall.Function);

                    var result = await _dispatcher.DispatchAsync(
                        toolCall.Function,
                        message,
                        context.CancellationToken);

                    if (result is not null)
                    {
                        outputs.Add(new ToolOutput(toolCall.Id, result.Content));
                    }
                }

                run = await _openAIClient.SubmitToolOutputs(
                    _options.ApiKey,
                    threadId,
                    run.Id,
                    new ToolsOutputsSubmissionBody(outputs),
                    context.CancellationToken);
            }
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
