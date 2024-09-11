using AIDemo.Web.Messages;
using Google.Apis.PeopleService.v1.Data;
using AIDemo.Entities;
using AIDemo.Library;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Haack.AIDemoWeb.Eventing.Consumers;

public class ContactImportConsumer(
    GoogleApiClient googleApiClient,
    IDbContextFactory<AIDemoDbContext> dbFactory) : IConsumer<ContactImportMessage>
{
    public async Task Consume(ConsumeContext<ContactImportMessage> context)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        await ImportContactsAsync(context, db);
    }

    async Task ImportContactsAsync(ConsumeContext<ContactImportMessage> context, AIDemoDbContext db)
    {
        var accessToken = context.Message.AccessToken;
        string? next = null;
        while (await googleApiClient.GetContactsAsync(accessToken, next) is { } contactsResponse)
        {
            foreach (var contact in contactsResponse.Connections)
            {
                // Import the contact only if we don't already have it.
                // TODO: Later we'll update existing contacts.
                var existing = await db.Contacts.FirstOrDefaultAsync(c => c.ResourceName == contact.ResourceName);
                if (existing is null)
                {
                    var addresses = contact.Addresses ?? new List<Address>();
                    var names = contact.Names ?? new List<Name>();
                    var emailAddresses = contact.EmailAddresses ?? new List<EmailAddress>();

                    // Look up locations for addresses.
                    var addressesWithLocations = await Task.WhenAll(addresses.Select(async address =>
                    {
                        var location = address.FormattedValue is { Length: > 0 }
                            ? await googleApiClient.GetLocationAsync(address.FormattedValue)
                            : null;
                        var point = location is not null
                            ? new Point(location.Geometry.Location.Lng, location.Geometry.Location.Lat) { SRID = 4326 }
                            : null;
                        return ContactAddress.FromGoogleContactAddress(address, point);
                    }));

                    // Create the contact
                    var newContact = new Contact
                    {
                        ResourceName = contact.ResourceName,
                        Birthday = contact.Birthdays?.FirstOrDefault() is { } birthday
                            ? ContactBirthday.FromGoogleContactBirthday(birthday)
                            : null,
                        Addresses = addressesWithLocations.ToList(),
                        Names = names.Select(ContactName.FromGoogleContactName).ToList(),
                        EmailAddresses = emailAddresses.Select(ContactEmailAddress.FromGoogleContactEmail).ToList(),
                    };
                    await db.Contacts.AddAsync(newContact);
                    await db.SaveChangesAsync(context.CancellationToken);
                }
            }

            if (contactsResponse.NextPageToken is null)
            {
                break;
            }
            next = contactsResponse.NextPageToken;
        }
    }
}

public record ContactImportMessage(string AccessToken);