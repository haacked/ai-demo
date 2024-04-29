using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;

namespace Haack.AIDemoWeb.Entities;

/// <summary>
/// A fact about a <see cref="Contact"/>
/// </summary>
public class ContactFact
{
    /// <summary>
    /// Id of the fact.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Content of the fact
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Embeddings for the fact.
    /// </summary>
    /// <remarks>
    /// The output dimensions for text-embedding-ada-002 is 1536.
    /// </remarks>
#pragma warning disable CA1002
    [Column(TypeName = "vector(1536 )")]
    public required Vector Embeddings { get; init; }
#pragma warning restore CA1002

    /// <summary>
    /// The justification for why the fact was added.
    /// </summary>
    public required string Justification { get; set; }

    /// <summary>
    /// The source message where the fact was stated.
    /// </summary>
    public required string Source { get; set; }

    /// <summary>
    /// The <see cref="Contact"/> the fact is about.
    /// </summary>
    public required Contact Contact { get; set; } = null!;

    /// <summary>
    /// The Id of the <see cref="Contact"/> the fact is about.
    /// </summary>
    public required int ContactId { get; set; }
}