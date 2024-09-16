namespace AIDemo.Web.Messages;

/// <summary>
/// A new message sent to a GPT chat bot.
/// </summary>
/// <remarks>
/// This message is consumed by BotMessageConsumer". Normally I wouldn't mention this here because
/// messages shouldn't be coupled to consumers, it's the other way around. But for demonstration purposes,
/// I mention it here so I can find the consumer for my demo more easily.
/// </remarks>
/// <param name="Message">The message text.</param>
/// <param name="Author">The name of author of the message.</param>
/// <param name="ConnectionId">The SignalR connection id</param>
/// <param name="UserIdentifier">The identifier of the user.</param>
public record BotMessageReceived(
    string Message,
    string Author,
    string ConnectionId,
    string? UserIdentifier);

/// <summary>
/// The role of a message in our system.
/// </summary>
public enum ChatMessageRole
{
    System,
    Assistant,
    User,
    Tool,
}


