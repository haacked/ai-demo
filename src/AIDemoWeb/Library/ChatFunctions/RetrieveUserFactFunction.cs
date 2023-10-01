using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Haack.AIDemoWeb.Entities;
using Haack.AIDemoWeb.Library;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OpenAIDemo.Hubs;

namespace Serious.ChatFunctions;

/// <summary>
/// Extends Chat GPT with a function that stores facts about a user.
/// </summary>
public class RetrieveUserFactFunction : ChatFunction<RetrieveUserFactArguments, object>
{
    readonly AIDemoContext _db;
    readonly OpenAIClientAccessor _client;
    readonly IHubContext<ChatHub> _hubContext;

    public RetrieveUserFactFunction(AIDemoContext db, OpenAIClientAccessor client, IHubContext<ChatHub> hubContext)
    {
        _db = db;
        _client = client;
        _hubContext = hubContext;
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

        // Grab the item with the highest cosine similarity.
        var facts = user.Facts
            .Select(fact => new
            {
                Fact = fact,
                Similarity = fact.Embeddings.CosineSimilarity(embeddings)
            })
            .Where(candidate => candidate.Similarity > 0.8)
            .OrderByDescending(candidate => candidate.Similarity)
            .ToList();

        if (facts.Count > 0)
        {
            var factSummary = string.Join("\n", facts.Select(f => $"Similarity: {f.Similarity}\tFact: {f.Fact.Content} Justification: {f.Fact.Justification} Source: {f.Fact.Source}"));

            await SendThought($"I have {facts.Count.ToQuantity("answer")} to that question\n\n{factSummary}");
            var answer = string.Join("\n", facts.Select(a => a.Fact.Content));
            return new UserFactResult(answer);
        }
        else
        {
            return new UserFactResult("I do not know");
        }

        async Task SendThought(string thought)
            => await _hubContext.Clients.All.SendAsync("thoughtReceived", thought, cancellationToken);
    }
}

public record UserFactResult(string Fact);

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