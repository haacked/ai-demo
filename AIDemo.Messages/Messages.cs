namespace AIDemoWeb.Entities.Eventing.Messages;

/// <summary>
/// A new message sent to an assistant.
/// </summary>
/// <param name="Message">The message text.</param>
/// <param name="AssistantName">The name of the assistant.</param>
/// <param name="AssistantId">The Id of the assistant.</param>
/// <param name="ThreadId">The Id of the assistant thread this message should be added to.</param>
/// <param name="ConnectionId">The SignalR connection id</param>
public record AssistantMessageReceived(
    string Message,
    string AssistantName,
    string AssistantId,
    string ThreadId,
    string ConnectionId);

/// <summary>
/// A new message sent to a GPT chat bot.
/// </summary>
/// <remarks>
/// This message is consumed by BotMessageConsumer". Normally I wouldn't mention this here because
/// messages shouldn't be coupled to consumers, it's the other way around. But for demonstration purposes,
/// I mention it here so I can find the consumer for my demo more easily.
/// </remarks>
/// <param name="Message">The message text.</param>
/// <param name="Author">The author of the message.</param>
/// <param name="ConnectionId">The SignalR connection id</param>
public record BotMessageReceived(
    string Message,
    string Author,
    string ConnectionId);
