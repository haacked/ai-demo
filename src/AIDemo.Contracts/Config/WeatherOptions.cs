namespace Haack.AIDemoWeb.Startup.Config;

/// <summary>
/// The configuration options for the Weather Service.
/// </summary>
public class WeatherOptions : IConfigOptions
{
    /// <summary>
    /// The Weather Service api key.
    /// </summary>
    public string? ApiKey { get; init; }

    /// <summary>
    /// The config section name. Leave null to use part of the name of this type before the "Options" suffix
    /// as the section name.
    /// </summary>
    public static string SectionName => "Weather";
}