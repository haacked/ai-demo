namespace Haack.AIDemoWeb.Entities;

/// <summary>
/// A fact about a <see cref="User"/>
/// </summary>
public class UserFact
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
    #pragma warning disable CA1002
    public required List<float> Embeddings { get; init; }
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
    /// The <see cref="User"/> the fact is about.
    /// </summary>
    public required User User { get; set; } = null!;

    /// <summary>
    /// The Id of the <see cref="User"/> the fact is about.
    /// </summary>
    public required int UserId { get; set; }
}