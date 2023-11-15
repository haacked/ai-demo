using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Azure.AI.OpenAI;
using Haack.AIDemoWeb.Entities;
using Microsoft.EntityFrameworkCore;
using Pgvector;

namespace Serious.ChatFunctions;

/// <summary>
/// Extends Chat GPT with a function that stores facts about a user.
/// </summary>
public class StoreUserFactFunction : ChatFunction<UserFactArguments, object>
{
    readonly AIDemoContext _db;
    readonly OpenAIClientAccessor _client;

    public StoreUserFactFunction(AIDemoContext db, OpenAIClientAccessor client)
    {
        _db = db;
        _client = client;
    }

    protected override string Name => "store_user_fact";
    protected override string Description => "Stores information about a user when a user says a fact about themself.";

    protected override async Task<object?> InvokeAsync(
        UserFactArguments arguments,
        string source,
        CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .Include(u => u.Facts)
            .FirstOrDefaultAsync(u => u.Name == arguments.Username, cancellationToken);

        if (user is not null
            // Make sure it's not already stored verbatim.
            && !user.Facts.Any(f => f.Content.Equals(arguments.Fact, StringComparison.OrdinalIgnoreCase)))
        {
            // Generate chat embedding for fact.
            var embeddings = await GetEmbeddingsAsync(arguments, cancellationToken);

            // TODO: Let's ask GPT if this fact is already known or changes an existing fact.
            user.Facts.Add(new UserFact
            {
                User = user,
                UserId = user.Id,
                Content = arguments.Fact,
                Embeddings = new Vector(embeddings),
                Justification = arguments.Justification,
                Source = source,
            });
            await _db.SaveChangesAsync(cancellationToken);
        }

        return null; // No need to respond.
    }

    async Task<float[]> GetEmbeddingsAsync(UserFactArguments arguments, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _client.GetEmbeddingsAsync(
                new EmbeddingsOptions { Input = new List<string> { arguments.Fact }},
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
/// The arguments to the weather service.
/// </summary>
public record UserFactArguments(
    [property: Required]
    [property: JsonPropertyName("username")]
    [property: Description("The username used to uniquely identify a user.")]
    string Username,

    [property: Required]
    [property: JsonPropertyName("fact")]
    [property: Description("The fact to store about the user.")]
    string Fact,

    [property: Required]
    [property: JsonPropertyName("justification")]
    [property: Description("Describe why you decided to call this function.")]
    string Justification);