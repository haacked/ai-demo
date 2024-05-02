using Google.Apis.PeopleService.v1.Data;
using Haack.AIDemoWeb.Entities;
using Haack.AIDemoWeb.Eventing.Consumers;
using Haack.AIDemoWeb.Library;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Serious;

namespace Haack.AIDemoWeb.Pages.Demos;

public class ContactsPageModel(
    GoogleApiClient googleApiClient,
    AIDemoContext db,
    IPublishEndpoint publishEndpoint) : PageModel
{
    public IReadOnlyList<Person> ApiContacts { get; private set; } = new List<Person>();

    public IReadOnlyList<Contact> Contacts { get; private set; } = null!;

    public int TotalImportedContacts { get; private set; }

    public int TotalImportedContactsWithAddresses { get; private set; }

    public int TotalImportedContactsWithAddressLocations { get; private set; }

    public int TotalImportedContactsWithFacts { get; private set; }


    public string? NextPageToken { get; private set; }

    public async Task OnGetAsync(bool showAll = false)
    {
        var contacts = await db
            .Contacts
            .Include(c => c.Facts)
            .Where(c => showAll || c.Addresses.Count > 0)
            .OrderByDescending(c => c.Facts.Count)
            .AsNoTracking()
            .ToListAsync();

        TotalImportedContacts = contacts.Count;
        TotalImportedContactsWithAddresses = contacts.Count(c => c.Addresses.Count != 0);
        TotalImportedContactsWithFacts = contacts.Count(c => c.Facts.Count != 0);
        TotalImportedContactsWithAddressLocations = contacts.Count(c => c.Addresses.Any(a => a.Location is not null));

        Contacts = contacts;
    }

    public async Task<IActionResult> OnPostAsync(string? next, bool showAll = false)
    {
        await OnGetAsync(showAll);

        var authenticateResult = await HttpContext.AuthenticateAsync("Google");
        var accessToken = authenticateResult.Properties.Require().GetTokenValue("access_token").Require();

        var connectionsResponse = await googleApiClient.GetContactsAsync(accessToken, next);
        ApiContacts = connectionsResponse.Connections.ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostImportAsync()
    {
        var authenticateResult = await HttpContext.AuthenticateAsync("Google");
        var accessToken = authenticateResult.Properties.Require().GetTokenValue("access_token").Require();

        // Starts a new import process
        await publishEndpoint.Publish(new ContactImportMessage(accessToken));

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Contacts\" CASCADE");
        return RedirectToPage();
    }
}