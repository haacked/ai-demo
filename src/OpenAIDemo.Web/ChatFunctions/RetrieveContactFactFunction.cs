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
/// Extends Chat GPT with a function that stores facts about a user.
/// </summary>
public class RetrieveContactFactFunction(
    AIDemoContext db,
    OpenAIClientAccessor client,
    IHubContext<BotHub> hubContext) : ChatFunction<RetrieveContactFactArguments, object>
{
    protected override string Name => "retrieve_contact_fact";

    protected override string Description =>
        """"
        Retrieves information when a person asks a question about a contact other than their location.
        
        For example, in the question:
        
        """
        What is Tyrion Lannister's favorite color?
        """
        
        The contact_name is Tyrion Lannister. 
        
        If someone asks
        
        """
        What is my son's favorite color?
        """
        
        We need to call this function with the current username to try and retrieve the username for the son.
        
        It may take more than one step to follow a set of connected facts in order to answer the question.
        
        If returning no results, just say you don't know.
        """";

    protected override async Task<object?> InvokeAsync(
        RetrieveContactFactArguments arguments,
        string source,
        CancellationToken cancellationToken)
    {
        var contactNames = arguments.ContactNames;

        // Query the database for the contacts(s).
        var contacts = await db.Contacts
            .Include(u => u.Facts)
            .Where(c => c.Names.Any(n => contactNames.Any(cn => EF.Functions.ILike(n.UnstructuredName, cn))))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (contacts is [])
        {
            return new ContactFactResults(Array.Empty<ContactFactResult>());
        }

        // Calculate the embedding for the question.
        var embeddings = await client.GetEmbeddingsAsync(arguments.Question, cancellationToken);

        if (embeddings is null)
        {
            return null;
        }

        var contactNameIdentifiers = contacts.Select(c => c.ResourceName).ToList();

        // Cosine similarity == 1 - Cosine Distance.
        var facts = await db.ContactFacts
            .Where(f => contactNameIdentifiers.Contains(f.Contact.ResourceName))
            .Select(f => new
            {
                Name = f.Contact.Names.FirstOrDefault(),
                Fact = f,
                Distance = f.Embeddings.CosineDistance(embeddings),
            })
            .Where(x => x.Distance <= 0.25)
            .OrderBy(x => x.Distance)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (facts.Count > 0)
        {
            var factSummary = string.Join("\n", facts.Select(f => $"Similarity: {1.0 - f.Distance} Fact: {f.Fact.Content} Justification: {f.Fact.Justification} Source: {f.Fact.Source}"));

            await SendThought($"I have {facts.Count.ToQuantity("answer")} to that question\n\n{factSummary}");
            return new ContactFactResults(facts.Select(f => new ContactFactResult(f.Name?.UnstructuredName ?? "Unknown", f.Fact.Content)).ToList());
        }

        return new ContactFactResults(Array.Empty<ContactFactResult>());

        async Task SendThought(string thought, string? data = null)
            => await hubContext.Clients.All.SendAsync("thoughtReceived", thought, data, cancellationToken);
    }
}

public record ContactFactResults(IReadOnlyList<ContactFactResult> ContactFacts);

public record ContactFactResult(string ContactName, string Fact);

/// <summary>
/// The arguments to the retrieve user fact function.
/// </summary>
public record RetrieveContactFactArguments(
    [property: Required]
    [property: JsonPropertyName("contact_names")]
    [property: Description("The name or names of contacts we want to retrieve facts for.")]
    IReadOnlyList<string> ContactNames,

    [property: Required]
    [property: JsonPropertyName("question")]
    [property: Description("The question being asked about a contact or set of contacts.")]
    string Question,

    [property: Required]
    [property: JsonPropertyName("justification")]
    [property: Description("Describe why you decided to call this function.")]
    string Justification);