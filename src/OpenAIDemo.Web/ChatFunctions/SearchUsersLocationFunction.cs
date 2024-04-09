using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Haack.AIDemoWeb.Entities;
using Haack.AIDemoWeb.Library;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using OpenAIDemo.Hubs;
using Coordinate = Haack.AIDemoWeb.Library.Coordinate;

namespace Serious.ChatFunctions;

/// <summary>
/// Extends Chat GPT with a function that searches for users who meet a condition.
/// </summary>
public class SearchUsersLocationFunction(
    AIDemoContext db,
    IHubContext<BotHub> hubContext) : ChatFunction<SearchUsersLocationArguments, object>
{
    protected override string Name => "search_users_location";

    protected override string Description =>
        """"
        When asking about all users that live near a location, this finds those users.
        """";

    protected override async Task<object?> InvokeAsync(
        SearchUsersLocationArguments arguments,
        string source,
        CancellationToken cancellationToken)
    {
        // Create a geometry from arguments.Coordinate.
        var point = new Point(arguments.Coordinate.Latitude, arguments.Coordinate.Longitude)
        {
            // 4326 is the SRID for the WGS84 spatial reference system, which is commonly used for latitude and
            // longitude coordinates.
            SRID = 4326
        };

        var users = await db.Users
            .Select(u => new { u.Name, u.Location, Distance = u.Location!.Distance(point) })
            .OrderBy(u => u.Distance)
            .ToListAsync(cancellationToken);

        if (users.Count > 0)
        {
            await SendThought($"I found {users.Count.ToQuantity("users")} near {arguments.Coordinate}.");

            var userDistances = users.Select(
                    u => new UserDistance(
                        u.Name,
                        new Measurement(
                            DistanceCalculator.CalculateDistance(
                                point.Coordinate.X,
                                point.Coordinate.Y,
                                u.Location!.X,
                                u.Location.Y), DistanceUnit.Kilometers)))
                .Where(u => u.Distance.Value <= arguments.Distance.Value)
                .ToList();

            return new SearchUsersLocationResult(userDistances);
        }

        return new SearchUsersLocationResult(Array.Empty<UserDistance>());

        async Task SendThought(string thought, string? data = null)
            => await hubContext.Clients.All.SendAsync("thoughtReceived", thought, data, cancellationToken);
    }
}

public record SearchUsersLocationResult(
    [property: JsonPropertyName("users")]
    IReadOnlyList<UserDistance> Users);

public record UserDistance(
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
    [property: Description("The coordinate we want to find users near or at. The `location_info` function can help you get this.")]
    Coordinate Coordinate,

    [property: Required]
    [property: JsonPropertyName("distance")]
    [property: Description("The distance in kilometers from the coordinate. If not specified, use 10 if the user is asking for nearby users. Otherwise use 1000. If the user specifies miles, convert to kilometers.")]
    Measurement Distance,

    [property: Required]
    [property: JsonPropertyName("justification")]
    [property: Description("Describe why you decided to call this function.")]
    string Justification);

