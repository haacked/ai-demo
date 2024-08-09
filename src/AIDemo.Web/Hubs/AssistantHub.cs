using AIDemo.Web.Messages;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using OpenAI.Assistants;

namespace OpenAIDemo.Hubs;

public class AssistantHub(IPublishEndpoint publishEndpoint, ILogger<AssistantHub> logger)
    : Hub
{
    public async Task Broadcast(
        string message,
        bool isUser,
        string assistantName,
        string assistantId,
        string threadId)
    {
        await Clients.Client(Context.ConnectionId).SendAsync(
            nameof(Broadcast),
            message,
            isUser,
            assistantName,
            assistantId,
            threadId,
            Array.Empty<TextAnnotation>());
        await publishEndpoint.Publish(
            new AssistantMessageReceived(
                message,
                assistantName,
                assistantId,
                threadId,
                Context.ConnectionId));
    }

    /// <summary>
    /// When the AI has thoughts about what it is doing, broadcast it to all clients.
    /// </summary>
    /// <param name="message">The thought.</param>
    /// <param name="data">Any additional data to format.</param>
    public async Task BroadcastThought(string message, string? data)
    {
        await Clients.Client(Context.ConnectionId).SendAsync(nameof(BroadcastThought), message, data);
    }

    /// <summary>
    /// When the AI is calling a function.
    /// </summary>
    /// <param name="name">The name of a function.</param>
    /// <param name="args">The arguments to the function.</param>
    public async Task BroadcastFunctionCall(string name, string args)
    {
        await Clients.Client(Context.ConnectionId).SendAsync(nameof(BroadcastFunctionCall), name, args);
    }

    public override Task OnConnectedAsync()
    {
        logger.Connected(Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.Disconnected(exception, Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}

public static partial class AssistantHubLoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "{ConnectionId} connected")]
    public static partial void Connected(this ILogger<AssistantHub> logger, string connectionId);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "{ConnectionId} disconnected")]
    public static partial void Disconnected(this ILogger<AssistantHub> logger, Exception? exception, string connectionId);
}