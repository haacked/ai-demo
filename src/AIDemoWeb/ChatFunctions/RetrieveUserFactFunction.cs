using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Haack.AIDemoWeb.Entities;
using Haack.AIDemoWeb.Library;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OpenAIDemo.Hubs;
using Pgvector.EntityFrameworkCore;

namespace Serious.ChatFunctions;

/// <summary>
/// Extends Chat GPT with a function that stores facts about a user.
/// </summary>
public class RetrieveUserFactFunction : ChatFunction<RetrieveUserFactArguments, object>
{
    readonly AIDemoContext _db;
    readonly OpenAIClientAccessor _client;
    readonly IHubContext<MultiUserChatHub> _hubContext;

    public RetrieveUserFactFunction(AIDemoContext db, OpenAIClientAccessor client, IHubContext<MultiUserChatHub> hubContext)
    {
        _db = db;
        _client = client;
        _hubContext = hubContext;
    }

    protected override string Name => "retrieve_user_fact";
    protected override string Description => "Retrieves information someone asks a question about user or themself.";

    protected override async Task<object?> InvokeAsync(
        RetrieveUserFactArguments arguments,
        string source,
        CancellationToken cancellationToken)
    {
        // Calculate the embedding for the question.
        var embeddings = await _client.GetEmbeddingsAsync(arguments.Question, cancellationToken);

        if (embeddings is null)
        {
            return null;
        }

        // Cosine similarity == 1 - Cosine Distance.
        var facts = await _db.UserFacts
            .Where(f => f.User.Name == arguments.Username)
            .Select(f => new
            {
                Fact = f,
                Distance = f.Embeddings.CosineDistance(embeddings),
            })
            .Where(x => x.Distance <= 0.25)
            .OrderBy(x => x.Distance)
            .ToListAsync(cancellationToken);

        if (facts.Count > 0)
        {
            var factSummary = string.Join("\n", facts.Select(f => $"Similarity: {1.0 - f.Distance} Fact: {f.Fact.Content} Justification: {f.Fact.Justification} Source: {f.Fact.Source}"));

            await SendThought($"I have {facts.Count.ToQuantity("answer")} to that question\n\n{factSummary}");
            var answer = string.Join("\n", facts.Select(a => a.Fact.Content));
            return new UserFactResult(answer);
        }

        return new UserFactResult("I do not know");

        async Task SendThought(string thought)
            => await _hubContext.Clients.All.SendAsync("thoughtReceived", thought, cancellationToken);
    }
}

public record UserFactResult(string Fact);

/// <summary>
/// The arguments to the retrieve user fact function.
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