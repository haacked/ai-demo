namespace AIDemoWeb.Entities.Eventing.Messages;

/// <summary>
/// A new chat message.
/// </summary>
/// <param name="Author">The author of the message.</param>
/// <param name="Message">The message text.</param>
public record MultiUserChatMessageReceived(string Author, string Message);