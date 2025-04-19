namespace Haack.AIDemoWeb.Startup.Config;

/// <summary>
/// The configuration options for Open AI.
/// </summary>
public class OpenAIOptions : IConfigOptions
{
    /// <summary>
    /// The Open AI api key.
    /// </summary>
    public string? ApiKey { get; init; }

    /// <summary>
    /// The Open AI Organization Id.
    /// </summary>
    public string? OrganizationId { get; init; }

    /// <summary>
    /// The model to use. Defaults to "gpt-4o".
    /// </summary>
    public string Model { get; init; } = "gpt-4o";

    /// <summary>
    /// The model to use for embeddings.
    /// </summary>
    public string EmbeddingModel { get; init; } = "text-embedding-3-large";

    /// <summary>
    /// The config section name. Leave null to use part of the name of this type before the "Options" suffix
    /// as the section name.
    /// </summary>
    public static string SectionName => "OpenAI";
}

