using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Azure.AI.OpenAI;
using Haack.AIDemoWeb.Entities;
using Haack.AIDemoWeb.Library;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Pgvector;

namespace Serious.ChatFunctions;

/// <summary>
/// Extends Chat GPT with a function that stores facts about a user.
/// </summary>
public class StoreUserFactFunction(AIDemoContext db, OpenAIClientAccessor client, GeocodeClient geocodeClient)
    : ChatFunction<UserFactArguments, object>
{
    protected override string Name => "store_user_fact";

    protected override string Description =>
        """
        Stores information about a person when a user makes a declarative statement or statements about another person or themself. 
        This only responds to declarative statements.

        For example, the statement \"@haacked's favorite color is blue\"

        results in the fact \"favorite color is blue\" stored for the username \"haacked\".

        If no username is specified, use the `retrieve_user_fact` function to retrieve the username.

        For example, the statement \"My daughter likes to draw.\", first retrieve the username for the current user's daughter and store the fact using the daughter's username. If no username for the daughter is found, ask for it.";
        """;

    public int Order => 2; // Comes after the StoreUserRelationshipFunction

    protected override async Task<object?> InvokeAsync(
        UserFactArguments arguments,
        string source,
        CancellationToken cancellationToken)
    {
        var username = arguments.Username.TrimLeadingCharacter('@');

        var user = await db.Users
            .Include(u => u.Facts)
            .FirstOrDefaultAsync(u => u.NameIdentifier == username, cancellationToken);

        if (user is null)
        {
            user = new User
            {
                NameIdentifier = username,
            };
            await db.Users.AddAsync(user, cancellationToken);
        }

        foreach (var fact in arguments.Facts)
        {
            if (
                // Make sure it's not already stored verbatim.
                !user.Facts.Any(f => f.Content.Equals(fact, StringComparison.OrdinalIgnoreCase)))
            {
                // Generate chat embedding for fact.
                var embeddings = await GetEmbeddingsAsync(fact, cancellationToken);

                if (arguments.Location is { Coordinate: { } coordinate } location)
                {
                    user.Location = new Point(coordinate.Latitude, coordinate.Longitude)
                    {
                        // 4326 is the SRID for the WGS84 spatial reference system, which is commonly used for latitude
                        // and longitude coordinates.
                        SRID = 4326
                    };
                    user.FormattedAddress = location.FormattedAddress;

                    if (await geocodeClient.GetTimeZoneAsync(coordinate.Latitude,
                            coordinate.Longitude)
                        is { } timeZoneId)
                    {
                        user.TimeZoneId = timeZoneId;
                    }
                }

                // TODO: Let's ask GPT if this fact is already known or changes an existing fact.
                user.Facts.Add(new UserFact
                {
                    User = user,
                    UserId = user.Id,
                    Content = fact,
                    Embeddings = new Vector(embeddings),
                    Justification = arguments.Justification,
                    Source = source,
                });
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        return null; // No need to respond.
    }

    async Task<float[]> GetEmbeddingsAsync(string fact, CancellationToken cancellationToken)
    {
        try
        {
            var response = await client.GetEmbeddingsAsync(
                new EmbeddingsOptions { Input = { fact } },
                cancellationToken);
            if (response.HasValue)
            {
                var embedding = response.Value.Data;
                if (embedding is { Count: > 0 })
                {
                    return embedding[0].Embedding.ToArray();
                }
            }
        }
#pragma warning disable CA1031
        catch (Exception)
#pragma warning restore CA1031
        {
        }
        return Array.Empty<float>();
    }
}

/// <summary>
/// The arguments to the store user fact function.
/// </summary>
public record UserFactArguments(
    [property: Required]
    [property: JsonPropertyName("username")]
    [property: Description("The username used to uniquely identify a user.")]
    string Username,

    [property: Required]
    [property: JsonPropertyName("facts")]
    [property: Description("The facts to store about the user.")]
    IReadOnlyList<string> Facts,

    [property: Required]
    [property: JsonPropertyName("location")]
    [property: Description("The location where the user lives if the fact is about the user's location. The `location_info` can be used to get this.")]
    UserLocation? Location,

    [property: Required]
    [property: JsonPropertyName("justification")]
    [property: Description("Describe why you decided to call this function.")]
    string Justification);