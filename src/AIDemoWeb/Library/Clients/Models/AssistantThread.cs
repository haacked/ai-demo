using System.Text.Json.Serialization;

namespace Haack.AIDemoWeb.Library.Clients;

public record AssistantThread
{
    /// <summary>
    /// The identifier, which can be referenced in API endpoints.
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
    /// Gets a set of 16 key-value pairs that can be attached to an object.
    /// This can be useful for storing additional information about the object in a structured format.
    /// Keys can be a maximum of 64 characters long and values can be a maximum of 512 characters long.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}