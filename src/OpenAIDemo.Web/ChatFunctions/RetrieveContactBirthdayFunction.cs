using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Haack.AIDemoWeb.Entities;
using Haack.AIDemoWeb.Library;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OpenAIDemo.Hubs;
using Serious.ChatFunctions;

namespace Haack.AIDemoWeb.ChatFunctions;

/// <summary>
/// Extends Chat GPT with a function that retrieves users birthday or looks up users with upcoming birthdays.
/// </summary>
public class RetrieveContactBirthdayFunction(
    AIDemoContext db,
    IHubContext<BotHub> hubContext) : ChatFunction<RetrieveContactBirthdayArguments, ContactBirthdayResults>
{
    protected override string Name => "retrieve_contact_birthday";

    protected override string Description =>
        """""
        Retrieves information about a contact's birthday or set of contacts with birthdays.
        
        For example, in the question:
        
        """
        When is Tyrion Lannister's birthday?
        """

        The contact_name is Tyrion Lannister and month is 0.

        If someone asks
        
        """
        Who's birthdays are in December?
        """
        
        The contact_name is empty and the month is 12.

        """
        Who's birthday is coming up?
        """

        The contact_name is empty and the month is 0.
        """""
    ;

    protected override async Task<ContactBirthdayResults?> InvokeAsync(
        RetrieveContactBirthdayArguments arguments,
        string source,
        CancellationToken cancellationToken)
    {
        // Look up birthdays for specific contacts.
        // Query the database for the contacts(s).
        var contacts = await GetContactsAsync(arguments, cancellationToken);

        var results = contacts.Select(ContactBirthdayResult.FromContact).ToList();

        var resultsSummary = string.Join("\n", results.Select(r => r.ContactName + " " + r.Birthday));

        await SendThought($"I have {results.Count.ToQuantity("birthdays")} to that question\n\n{resultsSummary}");

        return new ContactBirthdayResults(results);

        async Task SendThought(string thought, string? data = null)
            => await hubContext.Clients.All.SendAsync("thoughtReceived", thought, data, cancellationToken);
    }

    async Task<IReadOnlyList<Contact>> GetContactsAsync(RetrieveContactBirthdayArguments arguments, CancellationToken cancellationToken)
    {
        if (arguments.ContactNames is not [])
        {
            // Look up birthdays for specific contacts.
            // Query the database for the contacts(s).
            return await db.Contacts
                .Where(c => c.Names.Any(n => arguments.ContactNames.Any(cn => EF.Functions.ILike(n.UnstructuredName, cn))))
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        if (arguments.Month is > 0 and <= 12)
        {
            // Look up birthdays for contacts with birthdays in a specific month.
            return await db.Contacts
                .Where(c => c.Birthday!.Month == arguments.Month)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        if (arguments.Month == 0)
        {
            // Look up birthdays for contacts with upcoming birthdays in the next two weeks.
            var today = DateTime.Today;
            var twoWeeksFromToday = today.AddDays(14);

            return await db.Contacts
                .Where(c => c.Birthday!.Month == today.Month && c.Birthday.Day >= today.Day
                    || c.Birthday.Month == twoWeeksFromToday.Month && c.Birthday.Day <= twoWeeksFromToday.Day)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        return Array.Empty<Contact>();
    }
}

public record ContactBirthdayResults(IReadOnlyList<ContactBirthdayResult> ContactBirthdays);

public record ContactBirthdayResult(string ContactName, ContactBirthday Birthday)
{
    public static ContactBirthdayResult FromContact(Contact contact) =>
        new(
            contact.Names.First().UnstructuredName,
            contact.Birthday ?? new ContactBirthday(0, 0, 0));
}

/// <summary>
/// The arguments to the retrieve user fact function.
/// </summary>
public record RetrieveContactBirthdayArguments(
    [property: Required]
    [property: JsonPropertyName("contact_names")]
    [property: Description("The name or names of contacts we want to retrieve birthdays for.")]
    IReadOnlyList<string> ContactNames,

    [property: Required]
    [property: JsonPropertyName("month")]
    [property: Description("If this is a question about who has a birthday in a given month, this is the month being requested. This should be -1 if the question is about a specific contact or contacts birthday. This is should be 0 if the question is about upcoming birthdays.")]
    int Month,

    [property: Required]
    [property: JsonPropertyName("justification")]
    [property: Description("Describe why you decided to call this function.")]
    string Justification);