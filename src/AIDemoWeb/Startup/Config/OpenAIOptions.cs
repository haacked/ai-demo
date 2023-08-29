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
    public static void RegisterOpenAI(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OpenAIOptions>(configuration.GetSection(OpenAIOptions.OpenAI));
        services.AddSingleton<OpenAIClientAccessor>();
    }
}