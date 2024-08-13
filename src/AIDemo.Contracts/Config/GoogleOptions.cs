namespace Haack.AIDemoWeb.Startup.Config;

public class GoogleOptions : IConfigOptions
{
    /// <summary>
    /// The API Key for the Google geolocation service.
    /// </summary>
    public string? GeolocationApiKey { get; init; }

    /// <summary>
    /// The OAuth Client Id
    /// </summary>
    public string? OAuthClientId { get; init; }

    /// <summary>
    /// The OAuth Client Secret
    /// </summary>
    public string? OAuthClientSecret { get; init; }

    /// <summary>
    /// The config section name. Leave null to use part of the name of this type before the "Options" suffix
    /// as the section name.
    /// </summary>
    public static string SectionName => "Google";
}