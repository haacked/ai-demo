namespace AIDemoWeb.Entities.Eventing.Messages;

/// <summary>
/// A new chat message
/// </summary>
public record ChatMessage
{
    public required string Author { get; init; }

    public required string Message { get; init; }
}