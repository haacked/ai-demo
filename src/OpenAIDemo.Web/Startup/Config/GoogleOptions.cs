namespace Haack.AIDemoWeb.Startup.Config;

public class GoogleOptions
{
    public const string Google = nameof(Google);

    /// <summary>
    /// The API Key for the Google geolocation service.
    /// </summary>
    public string? GeolocationApiKey { get; init; }
}