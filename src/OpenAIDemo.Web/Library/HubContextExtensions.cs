using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using OpenAIDemo.Hubs;

namespace Haack.AIDemoWeb.Library;

public static class HubContextExtensions
{
    static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true };

    public static async Task BroadcastFunctionCall(
        this IHubContext<BotHub> hubContext,
        string name,
        KernelArguments args,
        CancellationToken cancellationToken = default)
    {
        var argsJson = JsonSerializer.Serialize(args, options: JsonSerializerOptions);
        await hubContext.Clients.All.SendAsync(
            nameof(BotHub.BroadcastFunctionCall),
            name,
            argsJson,
            cancellationToken);
    }

    public static async Task BroadcastFunctionResult(
        this IHubContext<BotHub> hubContext,
        string name,
        FunctionResult result,
        CancellationToken cancellationToken = default)
    {
        var thoughtMessage = $"I got a result from `{name}`. I'll send it back to GPT to summarize.";

        await hubContext.Clients.All.SendAsync(
            nameof(BotHub.BroadcastThought),
            thoughtMessage,
            JsonSerializer.Serialize(result.GetValue<object>(), options: JsonSerializerOptions),
            cancellationToken);
    }
}