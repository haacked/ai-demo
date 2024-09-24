using Haack.AIDemoWeb.Startup.Config;
using Microsoft.Extensions.Options;
using OpenAI;
using Serious;

namespace AIDemo.Library.Clients;

public static class ConfigOptionsExtensions
{
    public static IHostApplicationBuilder Configure<TOptions>(this IHostApplicationBuilder builder)
        where TOptions : class, IConfigOptions
    {
        var section = builder.Configuration.GetSection(TOptions.SectionName);
        builder.Services.Configure<TOptions>(section);
        return builder;
    }

    public static TOptions? GetConfigurationSection<TOptions>(this IHostApplicationBuilder builder)
        where TOptions : class, IConfigOptions
    {
        return builder.Configuration
            .GetSection(TOptions.SectionName)
            .Get<TOptions>();
    }
}

/// <summary>
/// Extensions to register the OpenAIOptions with the DI container.
/// </summary>
public static class OpenAIOptionsExtensions
{
    /// <summary>
    /// Register the OpenAIOptions with the DI container.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    public static IHostApplicationBuilder AddOpenAIClient(this IHostApplicationBuilder builder)
    {
        builder.Configure<OpenAIOptions>();
        builder.Services.AddSingleton(sp => new OpenAIClient(
            sp.GetRequiredService<IOptions<OpenAIOptions>>().Value.Require().ApiKey.Require()));

        return builder;
    }
}