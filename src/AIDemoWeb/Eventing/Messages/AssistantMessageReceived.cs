namespace AIDemoWeb.Entities.Eventing.Messages;

/// <summary>
/// A new message sent to an assistant.
/// </summary>
/// <param name="Message">The message text.</param>
/// <param name="AssistantName">The name of the assistant.</param>
/// <param name="AssistantId">The Id of the assistant.</param>
/// <param name="ThreadId">The Id of the assistant thread this message should be added to.</param>
public record AssistantMessageReceived(string Message, string AssistantName, string AssistantId, string ThreadId);