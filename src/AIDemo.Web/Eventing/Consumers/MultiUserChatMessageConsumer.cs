using AIDemoWeb.Entities.Eventing.Messages;
using Azure.AI.OpenAI;
using Haack.AIDemoWeb.Library;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using OpenAIDemo.Hubs;
using Serious;

namespace AIDemoWeb.Entities.Eventing.Consumers;

/// <summary>
/// This consumer is used for the multi-user chat demo which is legacy
/// and not one I use in talks.
/// </summary>
public class MultiUserChatMessageConsumer(
    IHubContext<MultiUserChatHub> hubContext,
    OpenAIClientAccessor client)
    : IConsumer<MultiUserChatMessageReceived>
{
    // We'll only maintain the last 20 messages in memory.
    static readonly LimitedQueue<ChatRequestMessage> Messages = new(20);

    public async Task Consume(ConsumeContext<MultiUserChatMessageReceived> context)
    {
        var (author, message) = context.Message;

        Messages.Enqueue(new ChatRequestUserMessage($"{author}:{message}"));

        if (message.StartsWith("Hey bot", StringComparison.OrdinalIgnoreCase))
        {
            await SendThought("The message addressed me! I'll try and respond.");
            var options = new ChatCompletionsOptions
            {
                Messages =
                {
                    new ChatRequestSystemMessage("You are observing a conversation in a chat room with multiple participants. Each message starts with the participant's name which must not be altered. If you can be of assistance, please do chime in, otherwise stay quiet and let them speak."),
                },
            };
            foreach (var chatMessage in Messages)
            {
                options.Messages.Add(chatMessage);
            }

            var response = await client.GetChatCompletionsAsync(options, context.CancellationToken);
            var responseChoice = response.Value.Choices[0];

            await SendResponseAsync(context, responseChoice.Message.Content);
        }
        else
        {
            await SendThought("The message didn't address me (by starting with \"Hey bot\") so I'll ignore it.");
        }

        return;

        async Task SendThought(string thought, string? data = null)
        {
            var thoughtMessage = thought;
            if (data is not null && data.StartsWith("\"{", StringComparison.Ordinal) &&
                                data.EndsWith("}\"", StringComparison.Ordinal))
            {
                var cleanData = data[1..^1].Replace(@"\u0022", "\"", StringComparison.Ordinal).JsonPrettify();
                thoughtMessage = thought + ":\n" + cleanData;
            }

            await hubContext.Clients.All.SendAsync(
                nameof(AssistantHub.BroadcastThought),
                thoughtMessage,
                context.CancellationToken);
        }
    }

    async Task SendResponseAsync(ConsumeContext<MultiUserChatMessageReceived> context, string response)
    {
        await hubContext.Clients.All.SendAsync(
            "messageReceived",
            "The Bot",
            response,
            context.CancellationToken);
        Messages.Enqueue(new ChatRequestAssistantMessage(response));
    }
}