namespace Haack.AIDemoWeb.Startup.Config;

/// <summary>
/// The configuration options for the Weather Service.
/// </summary>
public class WeatherOptions
{
    public const string Weather = "Weather";

    /// <summary>
    /// The Weather Service api key.
    /// </summary>
    public string? ApiKey { get; init; }
}