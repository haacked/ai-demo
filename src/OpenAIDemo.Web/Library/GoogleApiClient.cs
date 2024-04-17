using Google.Apis.Auth.OAuth2;
using Google.Apis.PeopleService.v1;
using Google.Apis.PeopleService.v1.Data;
using Google.Apis.Services;

namespace Haack.AIDemoWeb.Library;

public class GoogleApiClient
{
    static readonly IEnumerable<string> ContactsScopes = new[] { "https://www.googleapis.com/auth/contacts.readonly" };

    public async Task<ListConnectionsResponse> GetContactsAsync(string accessToken, string? nextPageToken = null)
    {
        var credential = GoogleCredential.FromAccessToken(accessToken)
            .CreateScoped(ContactsScopes);

        using var service = new PeopleServiceService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "Haack AI Demo"
        });

        // Retrieve the user's contacts
        var request = service.People.Connections.List("people/me");
        request.PersonFields = "names,emailAddresses,addresses,nicknames,userDefined,birthdays,phoneNumbers,metadata,photos";

        // Set the next page token if it's provided
        if (!string.IsNullOrEmpty(nextPageToken))
        {
            request.PageToken = nextPageToken;
        }
        request.SortOrder = PeopleResource.ConnectionsResource.ListRequest.SortOrderEnum.LASTMODIFIEDDESCENDING;

        return await request.ExecuteAsync();
    }
}