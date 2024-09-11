using AIDemo.Hubs;
using AIDemo.Web.Messages;
using AIDemo.Library;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using OpenAI;
using OpenAI.Assistants;
using Serious;

namespace AIDemoWeb.Entities.Eventing.Consumers;

public class AssistantMessageConsumer(IHubContext<AssistantHub> hubContext, OpenAIClient openAIClient)
    : IConsumer<AssistantMessageReceived>
{
    public async Task Consume(ConsumeContext<AssistantMessageReceived> context)
    {
        var (message, assistantName, assistantId, threadId, connectionId) = context.Message;

#pragma warning disable OPENAI001
        var assistantClient = openAIClient.GetAssistantClient();
#pragma warning restore OPENAI001

        // Create a new message in the thread
        var newMessage = await assistantClient.CreateMessageAsync(
            threadId.Require(),
            MessageRole.User,
            new[] { MessageContent.FromText(message), },
            cancellationToken: context.CancellationToken);

        var runCreateDate = DateTime.UtcNow;

        // Create a run for the thread.
        var run = (await assistantClient.CreateRunAsync(
            threadId,
            assistantId,
            cancellationToken: context.CancellationToken))
            .Value;

        var retryAttempt = 1;

        // Now we need to poll the run for the bot's response. We'll do it up to a minute.
        while (!run.Status.IsTerminal && DateTime.UtcNow < runCreateDate.AddMinutes(1))
        {
            if (retryAttempt is 4)
            {
                // We only want to send this message once and only if the bot is taking a while to respond.
                await SendResponseAsync("Thinking…");
            }

            await Task.Delay(1000, context.CancellationToken);
            run = await assistantClient.GetRunAsync(
                threadId,
                run.Id,
                context.CancellationToken);
            retryAttempt++;


            if (run.Status == RunStatus.RequiresAction)
            {
                await SendThought($"Assistant run {run.Id} needs me to call a function.");

                // TODO: Implement this later.
            }
        }

        if (run.Status == RunStatus.Completed)
        {
            // Grab the messages added by the assistant.
            var response = await assistantClient.GetMessagesAsync(
                threadId,
                new MessageCollectionOptions
                {
                    Order = ListOrder.OldestFirst,
                    AfterId = newMessage.Value.Id,
                },
                cancellationToken: context.CancellationToken)
                .GetAllValuesAsync(context.CancellationToken)
                .ToReadOnlyListAsync(context.CancellationToken);

            foreach (var reply in response.SelectMany(m => m.ToBlazorMessages()).Where(m => !m.IsUser))
            {
                await SendResponseAsync(reply.Text, reply.Annotations);
            }
        }
        else
        {
            await SendResponseAsync($"Sorry, I'm having trouble thinking right now: `{run.Status}`");
        }

        return;

        async Task SendThought(string thought, string? data = null)
            => await hubContext.Clients.Client(connectionId).SendAsync(
                nameof(AssistantHub.BroadcastThought),
                thought,
                data,
                context.CancellationToken);

        async Task SendResponseAsync(string response, IReadOnlyList<TextAnnotation>? annotations = null)
        {
            await hubContext.Clients.Client(connectionId).SendAsync(
                nameof(AssistantHub.Broadcast),
                response,
                false, // isUser
                assistantName,
                assistantId,
                threadId,
                annotations ?? Array.Empty<TextAnnotation>(),
                context.CancellationToken);
        }
    }
}
