using System.Text.Json.Serialization;

namespace Haack.AIDemoWeb.Library.Clients;

public record MessageCreateBody
{
    /// <summary>
    /// The entity that produced the message. One of <c>user</c> or <c>assistant</c>.
    /// </summary>
    public string Role { get; init; } = "user";

    /// <summary>
    /// The content of the message in array of text and/or images.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// A list of file IDs that the assistant should use. Useful for tools like retrieval and code_interpreter that
    /// can access files. A maximum of 10 files can be attached to a message.
    /// </summary>
    public IReadOnlyList<string> FileIds { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Set of 16 key-value pairs that can be attached to an object. This can be useful for storing additional
    /// information about the object in a structured format. Keys can be a maximum of 64 characters long and values can
    /// be a maximum of 512 characters long.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}

/// <summary>
/// A message associated with a thread.
/// </summary>
public record Message
{
    /// <summary>
    /// The message id.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// The object type, which is always <c>assistant</c>.
    /// </summary>
    [property: JsonPropertyName("object")]
    public required string ObjectType { get; init; }

    /// <summary>
    /// The Unix timestamp (in seconds) for when the assistant was created.
    /// </summary>
    public required long CreatedAt { get; init; }

    /// <summary>
    /// The thread ID that this message belongs to.
    /// </summary>
    public required string ThreadId { get; init; }

    /// <summary>
    /// If applicable, the ID of the assistant that authored this message.
    /// </summary>
    public string? AssistantId { get; init; }

    /// <summary>
    /// If applicable, the ID of the run associated with the authoring of this message.
    /// </summary>
    public string? RunId { get; init; }

    /// <summary>
    /// The entity that produced the message. One of <c>user</c> or <c>assistant</c>.
    /// </summary>
    public string Role { get; init; } = "user";

    /// <summary>
    /// The content of the message in array of text and/or images.
    /// </summary>
    public required IReadOnlyList<MessageContent> Content { get; init; }

    /// <summary>
    /// A list of file IDs that the assistant should use. Useful for tools like retrieval and code_interpreter that
    /// can access files. A maximum of 10 files can be attached to a message.
    /// </summary>
    public IReadOnlyList<string> FileIds { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Set of 16 key-value pairs that can be attached to an object. This can be useful for storing additional
    /// information about the object in a structured format. Keys can be a maximum of 64 characters long and values can
    /// be a maximum of 512 characters long.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}

/// <summary>
/// The content of a message.
/// </summary>
/// <param name="Type">The type of message.</param>
/// <param name="Text">The text of the message.</param>
public record MessageContent(string Type, MessageText Text);

/// <summary>
/// Text and annotations for a message.
/// </summary>
/// <param name="Value">The value of the message text.</param>
/// <param name="Annotations">Annotations?</param>
public record MessageText(string Value, IReadOnlyList<Annotation> Annotations);