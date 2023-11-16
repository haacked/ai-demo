using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Haack.AIDemoWeb.Library.Clients;

public record Assistant : AssistantCreateBody
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
}

/// <summary>
/// Represents the body of the request to create an assistant.
/// </summary>
public record AssistantCreateBody
{
    /// <summary>
    /// Gets the ID of the model to use.
    /// </summary>
    public required string Model { get; init; }

    /// <summary>
    /// Gets a list of tools enabled on the assistant. There can be a maximum of 128 tools per assistant.
    /// Tools can be of types <c>code_interpreter</c>, <c>retrieval</c>, or <c>function</c>.
    /// </summary>
    public IReadOnlyList<AssistantTool> Tools { get; init; } = Array.Empty<AssistantTool>();

    /// <summary>
    /// Gets a list of file IDs attached to this assistant. There can be a maximum of 20 files attached to the assistant.
    /// Files are ordered by their creation date in ascending order.
    /// </summary>
    public IReadOnlyList<string> FileIds { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets a set of 16 key-value pairs that can be attached to an object.
    /// This can be useful for storing additional information about the object in a structured format.
    /// Keys can be a maximum of 64 characters long and values can be a maximum of 512 characters long.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets the name of the assistant. Maximum length is 256 characters.
    /// </summary>
    [MaxLength(256)]
    public string? Name { get; init; }

    /// <summary>
    /// Gets the description of the assistant. The maximum length is 512 characters.
    /// </summary>
    [MaxLength(512)]
    public string? Description { get; init; }

    /// <summary>
    /// Gets the system instructions that the assistant uses. The maximum length is 32768 characters.
    /// </summary>
    [MaxLength(32768)]
    public string? Instructions { get; init; }
}

public record ObjectDeletedResponse(
    string Id,
    [property: JsonPropertyName("object")]
    string ObjectType,
    bool Deleted);