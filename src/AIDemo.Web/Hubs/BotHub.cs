using AIDemo.Web.Messages;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel.ChatCompletion;

namespace OpenAIDemo.Hubs;

public class BotHub(IPublishEndpoint publishEndpoint, ILogger<BotHub> logger) : Hub
{
    public async Task Broadcast(string message, string author, AuthorRole authorRole, string userIdentifier)
    {
        if (authorRole == AuthorRole.Assistant || authorRole == AuthorRole.User)
        {
            await Clients.Client(Context.ConnectionId).SendAsync(
                nameof(Broadcast),
                message,
                author,
                authorRole,
                userIdentifier);
        }

        // Publish the received message to the message bus, where the real action occurs.
        await publishEndpoint.Publish(new BotMessageReceived(
            message,
            author,
            Context.ConnectionId,
            userIdentifier));
    }

    /// <summary>
    /// When the AI has thoughts about what it is doing, broadcast it to all clients. This shows up in
    /// the browser's developer tools console.
    /// </summary>
    /// <param name="message">The thought.</param>
    /// <param name="data">Any additional data that should be formatted.</param>
    public async Task BroadcastThought(string message, string data)
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

public static partial class BotHubLoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "{ConnectionId} connected")]
    public static partial void Connected(this ILogger<BotHub> logger, string connectionId);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "{ConnectionId} disconnected")]
    public static partial void Disconnected(this ILogger<BotHub> logger, Exception? exception, string connectionId);
}