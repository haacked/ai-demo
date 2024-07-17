using Serious;

namespace Haack.AIDemoWeb.Startup.Config;

/// <summary>
/// The configuration options for Open AI.
/// </summary>
public class OpenAIOptions
{
    /// <summary>
    /// The name of the configuration section.
    /// </summary>
    public const string OpenAI = "OpenAI";

    /// <summary>
    /// The Open AI api key.
    /// </summary>
    public string? ApiKey { get; init; }

    /// <summary>
    /// The Open AI Organization Id.
    /// </summary>
    public string? OrganizationId { get; init; }

    /// <summary>
    /// The model to use. Defaults to "gpt-3.5-turbo".
    /// </summary>
    public string Model { get; init; } = "gpt-3.5-turbo";

    /// <summary>
    /// The model used for text retrieval within an assistant.
    /// </summary>
    public string RetrievalModel { get; init; } = "gpt-4-1106-preview";

    /// <summary>
    /// The model to use for embeddings.
    /// </summary>
    public string EmbeddingModel { get; init; } = "text-embedding-ada-002";
}

/// <summary>
/// Extensions to register the OpenAIOptions with the DI container.
/// </summary>
public static class OpenAIOptionsExtensions
{
    /// <summary>
    /// Register the OpenAIOptions with the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.  </param>
    public static IServiceCollection RegisterOpenAI(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OpenAIOptions>(configuration.GetSection(OpenAIOptions.OpenAI));
        services.AddSingleton<OpenAIClientAccessor>();

        return services;
    }
}