using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.Extensions.Options;
using Serious;

namespace Haack.AIDemoWeb.Library;

public class GeocodeClient(IGoogleGeocodeClient geocodeClient, IOptions<GoogleOptions> geocodeOptions)
{
    public  async Task<UserLocation?> GetLocationInformationAsync(string location)
    {
        var apiKey = geocodeOptions.Value.Require().GeolocationApiKey.Require();
        var response = await geocodeClient.GeoCodeAsync(apiKey, location);
        if (response.Results is { Count: 0 })
        {
            return null;
        }
        var result = response.Results[0];

        return new UserLocation(
            new(result.Geometry.Location.Lat, result.Geometry.Location.Lng),
            result.FormattedAddress);
    }

    public async Task<string?> GetTimeZoneAsync(double latitude, double longitude)
    {
        var apiKey = geocodeOptions.Value.Require().GeolocationApiKey.Require();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var response = await geocodeClient.GetTimeZoneAsync(apiKey, timestamp, latitude, longitude);
        return response.TimeZoneId;
    }
}

public record Coordinate(double Latitude, double Longitude);

public record UserLocation(
    Coordinate Coordinate,
    string FormattedAddress);