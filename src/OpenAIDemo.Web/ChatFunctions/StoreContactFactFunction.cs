using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Azure.AI.OpenAI;
using Haack.AIDemoWeb.Entities;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Serious;
using Serious.ChatFunctions;
// ReSharper disable All

namespace Haack.AIDemoWeb.ChatFunctions;

/// <summary>
/// Extends Chat GPT with a function that stores facts about a contact.
/// </summary>
public class StoreContactFactFunction(AIDemoContext db, OpenAIClientAccessor client)
    : ChatFunction<ContactFactArguments, object>
{
    protected override string Name => "store_contact_fact";

    protected override string Description =>
        """
        Stores information about a contact when a user makes a declarative statement or statements about another person. 
        This only responds to declarative statements.

        For example, the statement \"David Fowler's favorite color is blue\"

        results in the fact \"favorite color is blue\" stored for the name \"David Fowler\".

        If no username is specified, use the `retrieve_user_fact` function to retrieve the username.

        For example, the statement \"My daughter likes to draw.\", first retrieve the username for the current user's daughter and store the fact using the daughter's username. If no username for the daughter is found, ask for it.";
        """;

    public int Order => 2; // Comes after the StoreUserRelationshipFunction

    protected override async Task<object?> InvokeAsync(
        ContactFactArguments arguments,
        string source,
        CancellationToken cancellationToken)
    {
        var contact = await GetContactByNameAsync(arguments.ContactName, cancellationToken);
        if (contact is null)
        {
            return new { Message = "I don't know that contact" };
        }

        foreach (var fact in arguments.Facts)
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
                    Justification = arguments.Justification,
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
public record ContactFactArguments(
    [property: Required]
    [property: JsonPropertyName("contact_name")]
    [property: Description("The name of the contact.")]
    string ContactName,

    [property: Required]
    [property: JsonPropertyName("facts")]
    [property: Description("The facts to store about the contact.")]
    IReadOnlyList<string> Facts,

    [property: Required]
    [property: JsonPropertyName("justification")]
    [property: Description("Describe why you decided to call this function.")]
    string Justification);