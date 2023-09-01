using System.Text.Json;
using AIDemoWeb.Entities.Eventing.Messages;
using Azure.AI.OpenAI;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using OpenAIDemo.Hubs;
using Serious;
using Serious.ChatFunctions;

namespace AIDemoWeb.Entities.Eventing.Consumers;

public class ChatMessageConsumer : IConsumer<ChatMessageReceived>
{
    readonly IHubContext<ChatHub> _hubContext;
    readonly OpenAIClientAccessor _client;
    readonly WeatherFunction _weatherFunction;

    // We'll only maintain the last 20 messages in memory.
    static readonly LimitedQueue<ChatMessage> Messages = new(20);

    public ChatMessageConsumer(IHubContext<ChatHub> hubContext, OpenAIClientAccessor client, WeatherFunction weatherFunction)
    {
        _hubContext = hubContext;
        _client = client;
        _weatherFunction = weatherFunction;
    }

    public async Task Consume(ConsumeContext<ChatMessageReceived> context)
    {
        var message = context.Message;

        Messages.Enqueue(new ChatMessage(ChatRole.User, message.Message));

        if (message.Message.StartsWith("Hey bot", StringComparison.OrdinalIgnoreCase))
        {
            var options = new ChatCompletionsOptions
            {
                Messages =
                {
                    new ChatMessage(ChatRole.System, "You are observing a conversation in a chat room. If you can be of assistance, please do chime in."),
                },
                Functions = FunctionDefinitions.EnumerateFunctionDefinitions().ToList(),
            };
            foreach (var chatMessage in Messages)
            {
                options.Messages.Add(chatMessage);
            }
            var response = await _client.GetChatCompletionsAsync(options, context.CancellationToken);
            var responseChoice = response.Value.Choices[0];
            if (responseChoice.FinishReason == CompletionsFinishReason.FunctionCall)
            {
                if (responseChoice.Message.FunctionCall.Name == "get_current_weather")
                {
                    // Validate and process the JSON arguments for the function call
                    string unvalidatedArguments = responseChoice.Message.FunctionCall.Arguments;
                    var arguments = JsonSerializer.Deserialize<WeatherArguments>(unvalidatedArguments).Require();

                    // Actually call the weather service API.
                    var result = await _weatherFunction.GetWeatherAsync(arguments, context.CancellationToken);

                    // Serialize the result data from the function into a new chat message with the 'Function' role,
                    // then add it to the messages after the first User message and initial response FunctionCall
                    var content = JsonSerializer.Serialize(
                        result,
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    var functionResponseMessage = new ChatMessage(ChatRole.Function, content)
                    {
                        Name = "get_current_weather"
                    };
                    options.Messages.Add(functionResponseMessage);
                    Messages.Enqueue(functionResponseMessage);
                    // Now make a new request using all three messages in conversationMessages
                }

                response = await _client.GetChatCompletionsAsync(options, context.CancellationToken);
                responseChoice = response.Value.Choices[0];
            }
            var answer = responseChoice.Message.Content;
            await _hubContext.Clients.All.SendAsync("messageReceived", "The Bot", answer,
                context.CancellationToken);
        }
    }
}