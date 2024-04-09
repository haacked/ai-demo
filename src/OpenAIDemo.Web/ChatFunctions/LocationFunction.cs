using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Haack.AIDemoWeb.Entities;
using Haack.AIDemoWeb.Library;
using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Serious.ChatFunctions;

/// <summary>
/// Extends GPT with a function that can retrieve location or address information.
/// </summary>
public class LocationFunction(
    IGoogleGeocodeClient geocodeClient,
    AIDemoContext db,
    IOptions<GoogleOptions> geocodeOptions)
    : ChatFunction<UserLocationArguments, UserLocation>
{
    protected override string Name => "location_info";

    protected override string Description => "Retrieves location info any time a user mentions a location or address. For example, the statement \"I live at 123 Main St\" results in the location info for 123 Main St being retrieved. When asking about my own location, look it up based on my username.";

    public int Order => 1;

    protected override async Task<UserLocation?> InvokeAsync(
        UserLocationArguments arguments,
        string source,
        CancellationToken cancellationToken)
    {
        var apiKey = geocodeOptions.Value.Require().GeolocationApiKey.Require();
        var response = await geocodeClient.GeoCodeAsync(apiKey, arguments.Address);
        if (response.Results is { Count: 0 })
        {
            var username = arguments.Address.StartsWith('@') ? arguments.Address.TrimStart('@') : arguments.Address;

            // Maybe the address was a username. Look up the user's location.
            var user = await db.Users
                .FirstOrDefaultAsync(u => u.Name == username, cancellationToken);
            return user is { Location: { } location, FormattedAddress: { } formattedAddress }
                ? new UserLocation(new Coordinate(location.Coordinate.X, location.Coordinate.Y), formattedAddress)
                : new UserLocation(new Coordinate(0, 0), "Unknown");
        }
        var result = response.Results[0];

        return new UserLocation(
            new(result.Geometry.Location.Lat, result.Geometry.Location.Lng),
            result.FormattedAddress);
    }
}

public record UserLocationArguments(
    [property: Required]
    [property: JsonPropertyName("address")]
    [property: Description("The address or location.")]
    string Address);



public record UserLocation(
    [property: JsonPropertyName("coordinate")]
    Coordinate Coordinate,

    [property: JsonPropertyName("formatted_address")]
    string FormattedAddress);