using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Haack.AIDemoWeb.Library;
using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.Extensions.Options;
using Serious;
using Serious.ChatFunctions;

namespace Haack.AIDemoWeb.ChatFunctions;

/// <summary>
/// Extends GPT with a function that can retrieve location or address information.
/// </summary>
public class LocationFunction(
    IGoogleGeocodeClient geocodeClient,
    IOptions<GoogleOptions> geocodeOptions)
    : ChatFunction<ContactLocationArguments, ContactLocation>
{
    protected override string Name => "location_info";

    protected override string Description => "Retrieves location info any time a user mentions a location or address. For example, the statement \"I live at 123 Main St\" results in the location info for 123 Main St being retrieved.";

    public int Order => 1;

    protected override async Task<ContactLocation?> InvokeAsync(
        ContactLocationArguments arguments,
        string source,
        CancellationToken cancellationToken)
    {
        var apiKey = geocodeOptions.Value.Require().GeolocationApiKey.Require();
        var response = await geocodeClient.GeoCodeAsync(apiKey, arguments.Address);
        if (response.Results is { Count: 0 })
        {
            return new ContactLocation(new Coordinate(0, 0), "Unknown");
        }

        var result = response.Results[0];

        return new ContactLocation(
            new(result.Geometry.Location.Lat, result.Geometry.Location.Lng),
            result.FormattedAddress);
    }
}

public record ContactLocationArguments(
    [property: Required]
    [property: JsonPropertyName("address")]
    [property: Description("The address or location.")]
    string Address);



public record ContactLocation(
    [property: JsonPropertyName("coordinate")]
    Coordinate Coordinate,

    [property: JsonPropertyName("formatted_address")]
    string FormattedAddress);