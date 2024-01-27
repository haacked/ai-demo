using AIDemoWeb.Entities.Eventing.Messages;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace OpenAIDemo.Hubs;

public class BotHub : Hub
{
    readonly IPublishEndpoint _publishEndpoint;
    readonly ILogger<BotHub> _logger;

    public BotHub(IPublishEndpoint publishEndpoint, ILogger<BotHub> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Broadcast(string message, string author, bool isUser)
    {
        // TODO: This probably shouldn't be `Clients.All`
        await Clients.All.SendAsync(
            nameof(Broadcast),
            message,
            author,
            isUser);

        // Publish the received message to the message bus, where the real action occurs.
        await _publishEndpoint.Publish(new BotMessageReceived(message, author));
    }

    /// <summary>
    /// When the AI has thoughts about what it is doing, broadcast it to all clients. This shows up in
    /// the browser's developer tools console.
    /// </summary>
    /// <param name="message">The thought.</param>
    /// <param name="data">Any additional data that should be formatted.</param>
    public async Task BroadcastThought(string message, string data)
    {
        await Clients.All.SendAsync(nameof(BroadcastThought), message, data);
    }

    /// <summary>
    /// When the AI is calling a function.
    /// </summary>
    /// <param name="name">The name of a function.</param>
    /// <param name="args">The arguments to the function.</param>
    public async Task BroadcastFunctionCall(string name, string args)
    {
        await Clients.All.SendAsync(nameof(BroadcastFunctionCall), name, args);
    }

    public override Task OnConnectedAsync()
    {
        _logger.Connected(Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.Disconnected(exception, Context.ConnectionId);
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