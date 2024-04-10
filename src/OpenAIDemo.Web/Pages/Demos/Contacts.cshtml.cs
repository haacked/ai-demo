using Google.Apis.Auth.OAuth2;
using Google.Apis.PeopleService.v1;
using Google.Apis.PeopleService.v1.Data;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serious;

namespace AIDemoWeb.Demos.Pages;

public class ContactsPageModel : PageModel
{
    public IReadOnlyList<Person> Contacts { get; private set; } = null!;

    public string? NextPageToken { get; private set; }

    public async Task OnGetAsync(string? next)
    {
        var scopes = new[] { "https://www.googleapis.com/auth/contacts.readonly" };

        var authenticateResult = await HttpContext.AuthenticateAsync("Google");
        var accessToken = authenticateResult.Properties.Require().GetTokenValue("access_token");
        var credential = GoogleCredential.FromAccessToken(accessToken)
            .CreateScoped(scopes);

        using var service = new PeopleServiceService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "Haack AI Demo"
        });

        // Retrieve the user's contacts
        var request = service.People.Connections.List("people/me");
        request.PersonFields = "names,emailAddresses,addresses";
        // Set the next page token if it's provided
        if (!string.IsNullOrEmpty(next))
        {
            request.PageToken = next;
        }
        request.SortOrder = PeopleResource.ConnectionsResource.ListRequest.SortOrderEnum.LASTMODIFIEDDESCENDING;

        var connectionsResponse = await request.ExecuteAsync();
        Contacts = connectionsResponse.Connections.ToList();

        NextPageToken = connectionsResponse.NextPageToken;
    }
}