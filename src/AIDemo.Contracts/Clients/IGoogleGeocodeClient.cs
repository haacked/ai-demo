using Refit;

namespace Haack.AIDemoWeb.Library.Clients;

public interface IGoogleGeocodeClient
{
    public static Uri BaseAddress => new("https://maps.googleapis.com/maps/api");

    [Get("/geocode/json?key={apiKey}&address={address}")]
    Task<GoogleGeoCodingResponse> GeoCodeAsync(string apiKey, string address);


    [Get("/timezone/json?key={apiKey}&timestamp={timestamp}&location={latitude},{longitude}")]
    Task<GoogleTimeZoneResponse> GetTimeZoneAsync(string apiKey, long timestamp, double latitude, double longitude);
}