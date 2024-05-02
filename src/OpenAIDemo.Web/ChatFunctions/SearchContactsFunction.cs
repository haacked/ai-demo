using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Haack.AIDemoWeb.Entities;
using Haack.AIDemoWeb.Library;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OpenAIDemo.Hubs;
using Pgvector.EntityFrameworkCore;
using Serious;
using Serious.ChatFunctions;

namespace Haack.AIDemoWeb.ChatFunctions;

/// <summary>
/// Extends Chat GPT with a function that searches for users who meet a condition.
/// </summary>
public class SearchContactsFunction(
    AIDemoContext db,
    OpenAIClientAccessor client,
    IHubContext<BotHub> hubContext) : ChatFunction<SearchContactsArguments, object>
{
    protected override string Name => "search_contacts";

    protected override string Description =>
        """"
        When asking about all contacts that meet a criteria other than location, this finds or filters contacts that match the criteria.
        
        For example, when asking about all contacts that like whiskey, this function finds or filters contacts that like whiskey.
        """";

    protected override async Task<object?> InvokeAsync(
        SearchContactsArguments arguments,
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
        var factsQuery = arguments.ContactNames is not null or []
            ? db.ContactFacts.Where(f => arguments.ContactNames.Any(n => f.Contact.Names.Any(cn => EF.Functions.ILike(cn.UnstructuredName, n))))
            : db.ContactFacts;

        var facts = await factsQuery
            .Include(f => f.Contact)
            .Select(f => new
            {
                Fact = f,
                Distance = f.Embeddings.CosineDistance(embeddings),
            })
            .Where(x => x.Distance <= 0.18)
            .OrderBy(x => x.Distance)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (facts is [])
        {
            return new SearchContactsResult(Array.Empty<string>());
        }

        var searchSummary = string.Join("\n", facts.Select(f => $"Similarity: {1.0 - f.Distance} Fact: {f.Fact.Content} Justification: {f.Fact.Justification} Source: {f.Fact.Source}"));

        var contacts = facts.Select(f => f.Fact.Contact).Distinct().ToList();
        await SendThought($"I found {contacts.Count.ToQuantity("users")} that answer the question.\n\n{searchSummary}");

        return new SearchContactsResult(contacts.Select(u => u.Names.FirstOrDefault()?.UnstructuredName ?? "Unknown").ToList());

        async Task SendThought(string thought, string? data = null)
            => await hubContext.Clients.All.SendAsync("thoughtReceived", thought, data, cancellationToken);
    }
}

public record SearchContactsResult(IReadOnlyList<string> Contacts);

/// <summary>
/// The arguments to the retrieve user fact function.
/// </summary>
public record SearchContactsArguments(
    [property: Required]
    [property: JsonPropertyName("question")]
    [property: Description("The question being asked about a contact.")]
    string Question,


    [property: JsonPropertyName("contact_names")]
    [property: Description("If specified, only look up facts for these contacts.")]
    IReadOnlyList<string>? ContactNames,

    [property: Required]
    [property: JsonPropertyName("justification")]
    [property: Description("Describe why you decided to call this function.")]
    string Justification);