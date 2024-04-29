using AIDemoWeb.Entities.Eventing.Consumers;
using Google.Apis.PeopleService.v1.Data;
using Haack.AIDemoWeb.Entities;
using Haack.AIDemoWeb.Library;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Serious;

namespace AIDemoWeb.Demos.Pages;

public class ContactsPageModel(
    GoogleApiClient googleApiClient,
    AIDemoContext db,
    IPublishEndpoint publishEndpoint) : PageModel
{
    public IReadOnlyList<Person> Contacts { get; private set; } = null!;

    public int TotalImportedContacts { get; private set; }

    public int TotalImportedContactsWithAddresses { get; private set; }

    public int TotalImportedContactsWithAddressLocations { get; private set; }

    public int TotalImportedContactsWithFacts { get; private set; }


    public string? NextPageToken { get; private set; }

    public async Task OnGetAsync(string? next)
    {
        var contacts = await db.Contacts.Include(c => c.Facts).ToListAsync();
        TotalImportedContacts = contacts.Count;
        TotalImportedContactsWithAddresses = contacts.Count(c => c.Addresses.Count != 0);
        TotalImportedContactsWithFacts = contacts.Count(c => c.Facts.Count != 0);
        TotalImportedContactsWithAddressLocations = contacts.Count(c => c.Addresses.Any(a => a.Location is not null));

        var authenticateResult = await HttpContext.AuthenticateAsync("Google");
        var accessToken = authenticateResult.Properties.Require().GetTokenValue("access_token").Require();

        var connectionsResponse = await googleApiClient.GetContactsAsync(accessToken, next);
        Contacts = connectionsResponse.Connections.ToList();

        NextPageToken = connectionsResponse.NextPageToken;
    }

    public async Task<IActionResult> OnPostAsync()
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