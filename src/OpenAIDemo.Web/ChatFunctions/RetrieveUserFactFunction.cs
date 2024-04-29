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
public class RetrieveUserFactFunction(
    AIDemoContext db,
    OpenAIClientAccessor client,
    IHubContext<BotHub> hubContext) : ChatFunction<RetrieveUserFactArguments, object>
{
    protected override string Name => "retrieve_user_fact";

    protected override string Description =>
        """"
        Retrieves information when a person asks a question about their self or another user.
        
        Sometimes the other user is specified by username such as @haacked. 
        
        For example, in the question:
        
        """
        What is my favorite color?
        """
        
        The username is the current user.
        
        In the question:
        
        """
        What is @haacked's favorite color?
        """
        
        The username is haacked. 
        
        If someone asks
        
        """
        What is my son's favorite color?
        """
        
        We need to call this function with the current username to try and retrieve the username for the son.
        
        It may take more than one step to follow a set of connected facts in order to answer the question.
        
        If returning no results, just say you don't know.
        """";

    protected override async Task<object?> InvokeAsync(
        RetrieveUserFactArguments arguments,
        string source,
        CancellationToken cancellationToken)
    {


        var usernames = arguments.Usernames.Select(u => u.TrimLeadingCharacter('@')).ToList();

        // Query the database for the user(s).
        var users = await db.Users
            .Include(u => u.Facts)
            .Where(u => usernames.Contains(u.NameIdentifier))
            .ToListAsync(cancellationToken);

        if (users is [])
        {
            return new UserFactResults(Array.Empty<UserFactResult>());
        }

        // Calculate the embedding for the question.
        var embeddings = await client.GetEmbeddingsAsync(arguments.Question, cancellationToken);

        if (embeddings is null)
        {
            return null;
        }

        // Cosine similarity == 1 - Cosine Distance.
        var facts = await db.UserFacts
            .Where(f => usernames.Contains(f.User.NameIdentifier))
            .Select(f => new
            {
                Username = f.User.NameIdentifier,
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
            return new UserFactResults(facts.Select(f => new UserFactResult(f.Username, f.Fact.Content)).ToList());
        }

        return new UserFactResults(Array.Empty<UserFactResult>());

        async Task SendThought(string thought, string? data = null)
            => await hubContext.Clients.All.SendAsync("thoughtReceived", thought, data, cancellationToken);
    }
}

public record UserFactResults(IReadOnlyList<UserFactResult> UserFacts);

public record UserFactResult(string Username, string Fact);

/// <summary>
/// The arguments to the retrieve user fact function.
/// </summary>
public record RetrieveUserFactArguments(
    [property: Required]
    [property: JsonPropertyName("usernames")]
    [property: Description("The username or usernames who we want to retrieve facts for.")]
    IReadOnlyList<string> Usernames,

    [property: Required]
    [property: JsonPropertyName("question")]
    [property: Description("The question being asked about a user or users.")]
    string Question,

    [property: Required]
    [property: JsonPropertyName("justification")]
    [property: Description("Describe why you decided to call this function.")]
    string Justification);