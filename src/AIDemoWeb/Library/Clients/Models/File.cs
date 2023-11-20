using System.Text.Json.Serialization;

namespace Haack.AIDemoWeb.Library.Clients;

/// <summary>
/// A file uploaded to Open AI.
/// </summary>
public record File
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
    /// The number of bytes in the file.
    /// </summary>
    public required long Bytes { get; init; }

    /// <summary>
    /// The name of the file.
    /// </summary>
    public required string Filename { get; init; }

    /// <summary>
    /// The purpose of the file.
    /// </summary>
    public required string Purpose { get; init; }
}