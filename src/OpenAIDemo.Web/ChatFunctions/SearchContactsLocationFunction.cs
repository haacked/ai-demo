using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Haack.AIDemoWeb.Entities;
using Haack.AIDemoWeb.Library;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using OpenAIDemo.Hubs;
using Serious.ChatFunctions;
using Coordinate = Haack.AIDemoWeb.Library.Coordinate;

namespace Haack.AIDemoWeb.ChatFunctions;

/// <summary>
/// Extends Chat GPT with a function that searches for contacts near a location.
/// </summary>
public class SearchContactsLocationFunction(
    AIDemoContext db,
    IHubContext<BotHub> hubContext) : ChatFunction<SearchUsersLocationArguments, object>
{
    protected override string Name => "filter_contacts_by_location";

    protected override string Description =>
        """"
        When asking about all contacts that live near a location, this finds or filters those contacts.
        """";

    protected override async Task<object?> InvokeAsync(
        SearchUsersLocationArguments arguments,
        string source,
        CancellationToken cancellationToken)
    {
        // Create a geometry from arguments.Coordinate.
        var point = new Point(arguments.Coordinate.Longitude, arguments.Coordinate.Latitude)
        {
            // 4326 is the SRID for the WGS84 spatial reference system, which is commonly used for latitude and
            // longitude coordinates.
            SRID = 4326
        };

        var noContactNames = arguments.ContactNames is null or [];

        var contactsQuery = db.Contacts
            // We only look at contacts with a location.
            .Where(c => c.Addresses.Any(a => a.Location != null))
            .Select(c => new
            {
                Names = c.Names.Select(n => n.UnstructuredName),
                c.Addresses.FirstOrDefault(a => a.Location != null)!.Location,
            })
            .Select(c => new
            {
                c.Names,
                c.Location,
                Distance = c.Location!.Distance(point)
            })
            .Where(c => noContactNames || arguments.ContactNames!.Any(n => c.Names.Any(cn => EF.Functions.ILike(cn, n))))
            .OrderBy(u => u.Distance);

        var contacts = await contactsQuery.ToListAsync(cancellationToken);

        if (contacts is [])
        {
            return new SearchContactsLocationResult(Array.Empty<ContactDistance>());
        }

        await SendThought($"I found {contacts.Count.ToQuantity("users")} near {arguments.Coordinate}.");

        var contactDistances = contacts.Select(
                u => new ContactDistance(
                    u.Names.FirstOrDefault(n => noContactNames || arguments.ContactNames!.Contains(n)) ?? "Unknown",
                    new Measurement(
                        DistanceCalculator.CalculateDistance(
                            point.Coordinate.Y,
                            point.Coordinate.X,
                            u.Location!.Y,
                            u.Location.X), DistanceUnit.Kilometers)))
            .Where(u => u.Distance.Value <= arguments.Distance.Value)
            .ToList();

        return new SearchContactsLocationResult(contactDistances);

        async Task SendThought(string thought, string? data = null)
            => await hubContext.Clients.All.SendAsync("thoughtReceived", thought, data, cancellationToken);
    }
}

public record SearchContactsLocationResult(
    [property: JsonPropertyName("contacts")]
    IReadOnlyList<ContactDistance> Contacts);

public record ContactDistance(
    [property: JsonPropertyName("name")]
    string Name,

    [property: JsonPropertyName("distance")]
    Measurement Distance);

/// <summary>
/// The arguments to the retrieve user fact function.
/// </summary>
public record SearchUsersLocationArguments(
    [property: Required]
    [property: JsonPropertyName("coordinate")]
    [property: Description("The coordinate we want to find contacts near or at. The `location_info` function can help you get this.")]
    Coordinate Coordinate,

    [property: Required]
    [property: JsonPropertyName("distance")]
    [property: Description("The distance in kilometers from the coordinate. If not specified and asking about a country, use 610. If asking about near contacts, use 10. If the user specifies miles, convert to kilometers using the `unit_converter` function first.")]
    Measurement Distance,

    [property: JsonPropertyName("contact_names")]
    [property: Description("If specified, only look up the location for these contacts.")]
    IReadOnlyList<string>? ContactNames,

[property: Required]
    [property: JsonPropertyName("justification")]
    [property: Description("Describe why you decided to call this function.")]
    string Justification);

