using System.ComponentModel;
using AIDemo.Web.Messages;
using Haack.AIDemoWeb.Entities;
using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Serious;

namespace Haack.AIDemoWeb.SemanticKernel.Plugins;

/// <summary>
/// Extends GPT with a function that can retrieve location or address information.
/// </summary>
public class LocationPlugin(
    AIDemoDbContext db,
    IGoogleGeocodeClient geocodeClient,
    IOptions<GoogleOptions> geocodeOptions)
{
    [KernelFunction]
    [Description(
        "Retrieves location info any time a user mentions a location or address. For example, the statement \"I live at 123 Main St\" results in the location info for 123 Main St being retrieved.")]
    public async Task<ContactLocation?> GetContactLocationAsync(
        [Description("The address or location.")]
        string location,
        [Description("True if the location is a contact name. Otherwise false.")]
        bool isContact = false,
        CancellationToken cancellationToken = default)
    {
        if (isContact)
        {
            // Look up contact by name.
            var contact = await db.Contacts
                .Where(c => c.Addresses.Any(a => a.Location != null))
                .Where(c => c.Names.Any(n => EF.Functions.ILike(n.UnstructuredName, location)))
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (contact is null)
            {
                return new ContactLocation(new Coordinate(0, 0), "Unknown location");
            }

            var address = contact.Addresses.FirstOrDefault(a => a.Location != null);
            return address?.Location is not { } loc
                ? new ContactLocation(new Coordinate(0, 0), "Unknown location")
                : new ContactLocation(new(loc.Y, loc.X), address.FormattedValue ?? "Unspecified location");
        }

        var apiKey = geocodeOptions.Value.Require().GeolocationApiKey.Require();
        var response = await geocodeClient.GeoCodeAsync(apiKey, location);
        if (response.Results is { Count: 0 })
        {
            return new ContactLocation(new Coordinate(0, 0), "Unknown location");
        }

        var result = response.Results[0];

        return new ContactLocation(
            new(result.Geometry.Location.Lat, result.Geometry.Location.Lng),
            result.FormattedAddress);
    }
}

