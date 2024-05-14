namespace Haack.AIDemoWeb.Startup.Config;

public class GoogleOptions
{
    public const string Google = nameof(Google);

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
    /// The URL for the Google OAuth refresh token endpoint.
    /// </summary>
    public string? RefreshTokenEndpointUrl { get; init; }
}
