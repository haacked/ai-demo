using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Azure.AI.OpenAI;
using Haack.AIDemoWeb.Entities;
using Haack.AIDemoWeb.Library;
using Microsoft.EntityFrameworkCore;

namespace Serious.ChatFunctions;

/// <summary>
/// Extends Chat GPT with a function that stores facts about a user.
/// </summary>
public class RetrieveUserFactFunction : ChatFunction<RetrieveUserFactArguments, object>
{
    readonly AIDemoContext _db;
    readonly OpenAIClientAccessor _client;

    public RetrieveUserFactFunction(AIDemoContext db, OpenAIClientAccessor client)
    {
        _db = db;
        _client = client;
    }

    protected override string Name => "retrieve_user_fact";
    protected override string Description => "Retrieves information about a user when asked about a user.";

    protected override async Task<object?> InvokeAsync(
        RetrieveUserFactArguments arguments,
        string source,
        CancellationToken cancellationToken)
    {
        // Calculate the embedding for the question.
        var embeddings = await _client.GetEmbeddingsAsync(arguments.Question, cancellationToken);

        var user = await _db.Users
            .Include(u => u.Facts)
            .FirstOrDefaultAsync(u => u.Name == arguments.Username, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var cosineSimilarities = user.Facts
            .Select(f => f.Embeddings.CosineSimilarity(embeddings))
            .Where(similarity => similarity > 0)
            .OrderDescending()
            .ToList();




        return null; // No need to respond.
    }
}

/// <summary>
/// The arguments to the weather service.
/// </summary>
public record RetrieveUserFactArguments(
    [property: Required]
    [property: JsonPropertyName("username")]
    [property: Description("The username used to uniquely identify a user.")]
    string Username,

    [property: Required]
    [property: JsonPropertyName("question")]
    [property: Description("The question being asked about a user.")]
    string Question,

    [property: Required]
    [property: JsonPropertyName("justification")]
    [property: Description("Describe why you decided to call this function.")]
    string Justification);