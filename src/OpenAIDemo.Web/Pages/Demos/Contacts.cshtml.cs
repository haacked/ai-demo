using Google.Apis.PeopleService.v1.Data;
using Haack.AIDemoWeb.Library;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serious;

namespace AIDemoWeb.Demos.Pages;

public class ContactsPageModel(GoogleApiClient googleApiClient) : PageModel
{
    public IReadOnlyList<Person> Contacts { get; private set; } = null!;

    public string? NextPageToken { get; private set; }

    public async Task OnGetAsync(string? next)
    {
        var authenticateResult = await HttpContext.AuthenticateAsync("Google");
        var accessToken = authenticateResult.Properties.Require().GetTokenValue("access_token").Require();

        var connectionsResponse = await googleApiClient.GetContactsAsync(accessToken, next);
        Contacts = connectionsResponse.Connections.ToList();

        NextPageToken = connectionsResponse.NextPageToken;
    }
}