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
/// Extends Chat GPT with a function that searches for users who meet a condition.
/// </summary>
public class SearchUsersFunction(
    AIDemoContext db,
    OpenAIClientAccessor client,
    IHubContext<BotHub> hubContext) : ChatFunction<SearchUsersArguments, object>
{
    protected override string Name => "search_users";

    protected override string Description =>
        """"
        When asking about all users that meet a criteria, this searches for users that match the criteria.
        """";

    protected override async Task<object?> InvokeAsync(
        SearchUsersArguments arguments,
        string source,
        CancellationToken cancellationToken)
    {
        // Calculate the embedding for the question.
        var embeddings = await client.GetEmbeddingsAsync(arguments.Question, cancellationToken);

        if (embeddings is null)
        {
            return null;
        }

        // Cosine similarity == 1 - Cosine Distance.
        var facts = await db.UserFacts
            .Include(f => f.User)
            .Select(f => new
            {
                Fact = f,
                Distance = f.Embeddings.CosineDistance(embeddings),
            })
            .Where(x => x.Distance <= 0.18)
            .OrderBy(x => x.Distance)
            .ToListAsync(cancellationToken);

        if (facts.Count > 0)
        {
            var searchSummary = string.Join("\n", facts.Select(f => $"Similarity: {1.0 - f.Distance} Fact: {f.Fact.Content} Justification: {f.Fact.Justification} Source: {f.Fact.Source}"));

            var users = facts.Select(f => f.Fact.User).Distinct().ToList();
            await SendThought($"I found {users.Count.ToQuantity("users")} that answer the question.\n\n{searchSummary}");

            return new SearchUsersResult(users.Select(u => u.NameIdentifier).ToList());
        }

        return new SearchUsersResult(Array.Empty<string>());

        async Task SendThought(string thought, string? data = null)
            => await hubContext.Clients.All.SendAsync("thoughtReceived", thought, data, cancellationToken);
    }
}

public record SearchUsersResult(IReadOnlyList<string> Users);

/// <summary>
/// The arguments to the retrieve user fact function.
/// </summary>
public record SearchUsersArguments(
    [property: Required]
    [property: JsonPropertyName("question")]
    [property: Description("The question being asked about a user.")]
    string Question,

    [property: Required]
    [property: JsonPropertyName("justification")]
    [property: Description("Describe why you decided to call this function.")]
    string Justification);