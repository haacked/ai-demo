namespace Haack.AIDemoWeb.Startup.Config;

/// <summary>
/// Base interface for custom configuration sections.
/// </summary>
public interface IConfigOptions
{
    /// <summary>
    /// The name of the config section. If null, the part of the type name before "Options" is used
    /// as the section name by convention.
    /// </summary>
    static abstract string SectionName { get; }
}

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

