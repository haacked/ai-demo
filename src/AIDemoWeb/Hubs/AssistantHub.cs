using AIDemoWeb.Entities.Eventing.Messages;
using Haack.AIDemoWeb.Library.Clients;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace OpenAIDemo.Hubs;

public class AssistantHub : Hub
{
    readonly IPublishEndpoint _publishEndpoint;
    readonly ILogger<AssistantHub> _logger;

    public AssistantHub(IPublishEndpoint publishEndpoint, ILogger<AssistantHub> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Broadcast(string message, bool isUser, string assistantName, string assistantId, string threadId)
    {
        await Clients.All.SendAsync("Broadcast", message, isUser, assistantName, assistantId, threadId, Array.Empty<Annotation>());
        await _publishEndpoint.Publish(new AssistantMessageReceived(message, assistantName, assistantId, threadId));
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