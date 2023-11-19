namespace Haack.AIDemoWeb.Library.Clients;

/// <summary>
/// A message that is part of a thread.
/// </summary>
/// <param name="Role">The role of the entity that is creating the message. Currently only <c>user</c> is supported.</param>
/// <param name="Content">The content of the message.</param>
/// <param name="FileIds">A list of File IDs that the message should use. There can be a maximum of 10 files attached to a message. Useful for tools like retrieval and code_interpreter that can access and use files.</param>
/// <param name="Metadata">Set of 16 key-value pairs that can be attached to an object. This can be useful for storing additional information about the object in a structured format. Keys can be a maximum of 64 characters long and values can be a maxium of 512 characters long.</param>
public record Message(
    string Role,
    string Content,
    IReadOnlyList<string> FileIds,
    IReadOnlyDictionary<string, string> Metadata)
{
    /// <summary>
    /// Convenience constructor.
    /// </summary>
    /// <param name="Content">The content of the message.</param>
    public Message(string Content) : this(Content, Array.Empty<string>()) { }

    /// <summary>
    /// Convenience constructor.
    /// </summary>
    /// <param name="Content">The content of the message.</param>
    /// <param name="FileIds">A list of File IDs that the message should use. There can be a maximum of 10 files attached to a message. Useful for tools like retrieval and code_interpreter that can access and use files.</param>
    public Message(string Content, IReadOnlyList<string> FileIds)
        : this("user", Content, Array.Empty<string>(), new Dictionary<string, string>()) { }
};
