using AIDemoWeb.Entities.Eventing.Messages;
using Haack.AIDemoWeb.Library;
using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using OpenAIDemo.Hubs;
using Serious;

namespace AIDemoWeb.Entities.Eventing.Consumers;

public class AssistantMessageConsumer : IConsumer<AssistantMessageReceived>
{
    readonly IHubContext<AssistantHub> _hubContext;
    readonly IOpenAIClient _openAIClient;
    readonly OpenAIOptions _options;


    public AssistantMessageConsumer(
        IHubContext<AssistantHub> hubContext,
        IOpenAIClient openAIClient,
        IOptions<OpenAIOptions> options)
    {
        _hubContext = hubContext;
        _openAIClient = openAIClient;
        _options = options.Value;
    }

    public async Task Consume(ConsumeContext<AssistantMessageReceived> context)
    {
        var (message, assistantName, assistantId, threadId) = context.Message;

        // Create a new message in the thread
        var newMessage = await _openAIClient.CreateMessageAsync(
            _options.ApiKey.Require(),
            threadId,
            new MessageCreateBody { Content = message },
            context.CancellationToken);

        var runCreateDate = DateTime.UtcNow;

        // Create a run for the thread.
        var run = await _openAIClient.CreateRunAsync(
            _options.ApiKey.Require(),
            threadId,
            new ThreadRunCreateBody
            {
                AssistantId = assistantId
            },
            context.CancellationToken);

        var retryDelayMultiplier = 1;

        // Now we need to poll the run for the bot's response. We'll do it up to a minute.
        while (run.Status is not ("completed" or "cancelled" or "failed" or "expired") && DateTime.UtcNow < runCreateDate.AddMinutes(1))
        {
            if (retryDelayMultiplier is 4)
            {
                // We only want to send this message once and only if the bot is taking a while to respond.
                await SendResponseAsync("Thinking…");
            }
            await Task.Delay(1000 * retryDelayMultiplier, context.CancellationToken);
            run = await _openAIClient.GetRunAsync(
                _options.ApiKey.Require(),
                run.Id,
                threadId,
                context.CancellationToken);
            retryDelayMultiplier *= 2;
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
                await SendResponseAsync(reply.Text);
            }
        }
        else
        {
            await SendResponseAsync($"Sorry, I'm having trouble thinking right now: `{run.Status}`");
        }

        return;

        async Task SendResponseAsync(string response)
        {
            await _hubContext.Clients.All.SendAsync(
                nameof(AssistantHub.Broadcast),
                response,
                false, // isUser
                assistantName,
                assistantId,
                threadId,
                context.CancellationToken);
        }
    }
}