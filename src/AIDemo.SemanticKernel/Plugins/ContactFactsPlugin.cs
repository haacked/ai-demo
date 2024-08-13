using System.ComponentModel;
using AIDemo.Hubs;
using AIDemo.Web.Messages;
using Haack.AIDemoWeb.Entities;
using Haack.AIDemoWeb.Library;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace Haack.AIDemoWeb.Plugins;

#pragma warning disable SKEXP0001
public class ContactFactsPlugin(
    AIDemoDbContext db,
    ITextEmbeddingGenerationService embeddingClient,
    IHubContext<BotHub> hubContext)
#pragma warning restore SKEXP0001
{
    [KernelFunction]
    [Description(
        """"
        Retrieves information when a person asks a question about a contact other than their location or birthday.

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
        """")]
    public async Task<IReadOnlyList<ContactFactResult>> GetFactsAsync(
        [Description("The name or names of contacts we want to retrieve facts for.")]
        IReadOnlyList<string> contactNames,
        [Description("The question being asked about a contact or set of contacts.")]
        string question,
        [Description("Describe why you decided to call this function.")]
        string justification,
        CancellationToken cancellationToken)
    {
        // Query the database for the contacts(s).
        var contacts = await db.Contacts
            .Include(u => u.Facts)
            .Where(c => c.Names.Any(n => contactNames.Any(cn => EF.Functions.ILike(n.UnstructuredName, cn))))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (contacts is [])
        {
            return Array.Empty<ContactFactResult>();
        }

        // Calculate the embedding for the question.
#pragma warning disable SKEXP0001
        var embeddings = await embeddingClient.GenerateEmbeddingAsync(
            question,
            cancellationToken: cancellationToken);
#pragma warning restore SKEXP0001

        if (embeddings.Length is 0)
        {
            return [];
        }

        var embeddingVector = new Vector(embeddings);

        var contactNameIdentifiers = contacts.Select(c => c.ResourceName).ToList();

        // Cosine similarity == 1 - Cosine Distance.
        var facts = await db.ContactFacts
            .Where(f => contactNameIdentifiers.Contains(f.Contact.ResourceName))
            .Select(f => new
            {
                Name = f.Contact.Names.FirstOrDefault(),
                Fact = f,
                Distance = f.Embeddings.CosineDistance(embeddingVector),
            })
            .Where(x => x.Distance <= 0.25)
            .OrderBy(x => x.Distance)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (facts.Count > 0)
        {
            var factSummary = string.Join("\n", facts.Select(f => $"Similarity: {1.0 - f.Distance} Fact: {f.Fact.Content} Justification: {f.Fact.Justification} Source: {f.Fact.Source}"));

            await SendThought($"I have {facts.Count.ToQuantity("answer")} to that question\n\n{factSummary}");
            return facts.Select(f => new ContactFactResult(f.Name?.UnstructuredName ?? "Unknown", f.Fact.Content)).ToList();
        }

        return Array.Empty<ContactFactResult>();

        async Task SendThought(string thought, string? data = null)
            => await hubContext.Clients.All.SendAsync("thoughtReceived", thought, data, cancellationToken);
    }

    [KernelFunction]
    [Description(
        """"
        When asking about all contacts that meet a criteria other than location, this finds or filters contacts that match the criteria.
        
        For example, when asking about all contacts that like whiskey, this function finds or filters contacts that like whiskey.
        """")]
    public async Task<object?> SearchContactsAsync(
        [Description("The question being asked about a contact.")]
        string question,
        [Description("If specified, only look up facts for these contacts.")]
        IReadOnlyList<string>? contactNames,
        [Description("Describe why you decided to call this function.")]
        string justification,
        CancellationToken cancellationToken)
    {
        // Calculate the embedding for the question.
#pragma warning disable SKEXP0001
        var embeddings = await embeddingClient.GenerateEmbeddingAsync(
            question,
            cancellationToken: cancellationToken);
#pragma warning restore SKEXP0001

        if (embeddings.Length is 0)
        {
            return null;
        }

        // Cosine similarity == 1 - Cosine Distance.
        var factsQuery = contactNames is not null or []
            ? db.ContactFacts.Where(f => contactNames.Any(n => f.Contact.Names.Any(cn => EF.Functions.ILike(cn.UnstructuredName, n))))
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

        var contacts = facts.Select(f => f.Fact.Contact).Distinct().ToList();

        return new SearchContactsResult(contacts.Select(u => u.Names.FirstOrDefault()?.UnstructuredName ?? "Unknown").ToList());
    }

    [KernelFunction]
    [Description(
        """
        Stores information about a contact when a user makes a declarative statement or statements about another person. 
        This only responds to declarative statements.

        For example, the statement \"David Fowler's favorite color is blue\"

        results in the fact \"favorite color is blue\" stored for the name \"David Fowler\".

        If no username is specified, use the `retrieve_user_fact` function to retrieve the username.

        For example, the statement \"My daughter likes to draw.\", first retrieve the username for the current user's daughter and store the fact using the daughter's username. If no username for the daughter is found, ask for it.";
        """)]
    public async Task<object?> StoreFactsAsync(
        [Description("The name of the contact.")]
        string contactName,
        [Description("The facts to store about the contact.")]
        IReadOnlyList<string> facts,
        [Description("The source message stating this fact.")]
        string source,
        [Description("Describe why you decided to call this function.")]
        string justification,
        CancellationToken cancellationToken)
    {
        var contact = await GetContactByNameAsync(contactName, cancellationToken);
        if (contact is null)
        {
            return new { Message = "I don't know that contact" };
        }

        foreach (var fact in facts)
        {
            if (
                // Make sure it's not already stored verbatim.
                !contact.Facts.Any(f => f.Content.Equals(fact, StringComparison.OrdinalIgnoreCase)))
            {
                // Generate chat embedding for fact.
                var embeddings = await GetEmbeddingsAsync(fact, cancellationToken);

                // TODO: Let's ask GPT if this fact is already known or changes an existing fact.
                contact.Facts.Add(new ContactFact
                {
                    Contact = contact,
                    ContactId = contact.Id,
                    Content = fact,
                    Embeddings = new Vector(embeddings),
                    Justification = justification,
                    Source = source,
                });
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        return null; // No need to respond.
    }

    Task<Contact?> GetContactByNameAsync(string contactName, CancellationToken cancellationToken)
    {
        return db.Contacts
            .Include(c => c.Facts)
            .FirstOrDefaultAsync(c => c.Names.Any(n => EF.Functions.ILike(n.UnstructuredName, contactName)), cancellationToken);
    }

    async Task<ReadOnlyMemory<float>> GetEmbeddingsAsync(string fact, CancellationToken cancellationToken)
    {
        try
        {
#pragma warning disable SKEXP0001
            var response = await embeddingClient.GenerateEmbeddingAsync(
            fact,
            cancellationToken: cancellationToken);
#pragma warning restore SKEXP0001
            return response;
        }
#pragma warning disable CA1031
        catch (Exception)
#pragma warning restore CA1031
        {
        }
        return new ReadOnlyMemory<float>();
    }
}
