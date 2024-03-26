using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.Extensions.Options;

namespace Serious.ChatFunctions;

/// <summary>
/// Extends GPT with a function that can retrieve location or address information.
/// </summary>
public class LocationFunction(IGoogleGeocodeClient geocodeClient, IOptions<GoogleOptions> geocodeOptions)
    : ChatFunction<UserLocationArguments, UserLocationResult>
{
    protected override string Name => "location_info";
    protected override string Description => "Retrieves location info any time a user mentions a location or address. For example, the statement \"I live at 123 Main St\" results in the location info for 123 Main St being retrieved.";
    protected override async Task<UserLocationResult?> InvokeAsync(
        UserLocationArguments arguments,
        string source,
        CancellationToken cancellationToken)
    {
        var apiKey = geocodeOptions.Value.Require().GeolocationApiKey.Require();
        var response = await geocodeClient.GeoCodeAsync(apiKey, arguments.Address);
        if (response.Results is { Count: 0 })
        {
            return null;
        }
        var result = response.Results[0];

        return new UserLocationResult(
            new(result.Geometry.Location.Lat, result.Geometry.Location.Lng),
            result.FormattedAddress);
    }
}

public record UserLocationArguments(
    [property: Required]
    [property: JsonPropertyName("address")]
    [property: Description("The address or location.")]
    string Address);

public record Coordinate(double Latitude, double Longitude);

public record UserLocationResult(
    Coordinate Location,
    string FormattedAddress);