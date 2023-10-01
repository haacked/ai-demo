namespace AIDemoWeb.Entities.Eventing.Messages;

/// <summary>
/// A new chat message.
/// </summary>
public record ChatMessageReceived
{
    /// <summary>
    /// The author of the message.
    /// </summary>
    public required string Author { get; init; }

    /// <summary>
    /// The text of the message.
    /// </summary>
    public required string Message { get; init; }
}