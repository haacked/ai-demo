using Refit;

namespace Haack.AIDemoWeb.Library.Clients;

public interface IGoogleGeocodeClient
{
    public static Uri BaseAddress => new("https://maps.googleapis.com/maps/api/");

    [Get("/geocode/json?key={apiKey}&address={address}")]
    Task<GoogleGeoCodingResponse> GeoCodeAsync(string apiKey, string address);


    [Get("/timezone/json?key={apiKey}&timestamp={timestamp}&location={latitude},{longitude}")]
    Task<GoogleTimeZoneResponse> GetTimeZoneAsync(string apiKey, long timestamp, double latitude, double longitude);
}

public static class GoogleGeocodeClientExtensions
{
    public static async Task<GoogleTimeZoneResponse> GetTimeZoneAsync(this IGoogleGeocodeClient client, string apiKey, double latitude, double longitude)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return await client.GetTimeZoneAsync(apiKey, timestamp, latitude, longitude);
    }
}
