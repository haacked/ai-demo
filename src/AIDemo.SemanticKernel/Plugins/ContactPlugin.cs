using System.ComponentModel;
using AIDemo.Web.Messages;
using AIDemo.Entities;
using AIDemo.Library.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using NetTopologySuite.Geometries;
using Coordinate = AIDemo.Web.Messages.Coordinate;

namespace Haack.AIDemoWeb.SemanticKernel.Plugins;

/// <summary>
/// Extends Chat GPT with a function that retrieves contacts birthday or looks up contacts with upcoming birthdays.
/// </summary>
public class ContactPlugin(AIDemoDbContext db)
{
    [KernelFunction]
    [Description(
        """"
        When asking about all contacts that live near a location, this finds or filters those contacts.
        """")]
    public async Task<IReadOnlyList<ContactDistance>> SearchContactsInLocationAsync(
        [Description("The coordinate we want to find contacts near or at. The `location_info` function can help you get this.")]
        Coordinate coordinate,
        [Description("The distance in kilometers from the coordinate. If the distance is not specified, use 610 km for the distance. If the user specifies miles, convert to kilometers using the `unit_converter` function first.")]
        Measurement<DistanceUnit> distance,
        [Description("If specified, only look up the location for these contacts.")]
        IReadOnlyList<string>? contactNames,
        [Description("Describe why you decided to call this function.")]
        string justification,
        CancellationToken cancellationToken)
    {
        // Create a geometry from arguments.Coordinate.
        var point = new Point(coordinate.Longitude, coordinate.Latitude)
        {
            // 4326 is the SRID for the WGS84 spatial reference system, which is commonly used for latitude and
            // longitude coordinates.
            SRID = 4326
        };

        var noContactNames = contactNames is null or [];

        var contactsQuery = db.Contacts
            // We only look at contacts with a location.
            .Where(c => c.Addresses.Any(a => a.Location != null))
            .Select(c => new
            {
                Names = c.Names.Select(n => n.UnstructuredName),
                c.Addresses.FirstOrDefault(a => a.Location != null)!.Location,
            })
            .Select(c => new
            {
                c.Names,
                c.Location,
                Distance = c.Location!.Distance(point)
            })
            .Where(c => noContactNames || contactNames!.Any(n => c.Names.Any(cn => EF.Functions.ILike(cn, n))))
            .OrderBy(u => u.Distance);

        var contacts = await contactsQuery.ToListAsync(cancellationToken);

        if (contacts is [])
        {
            return [];
        }

        return contacts.Select(
                u => new ContactDistance(
                    u.Names.FirstOrDefault(n => noContactNames || contactNames!.Contains(n)) ?? "Unknown",
                    new Measurement<DistanceUnit>(
                        DistanceCalculator.CalculateDistance(
                            point.Coordinate.Y,
                            point.Coordinate.X,
                            u.Location!.Y,
                            u.Location.X), DistanceUnit.Kilometers)))
            .Where(u => u.Distance.Value <= distance.Value)
            .ToList();
    }

    [KernelFunction]
    [Description(
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
    )]
    public async Task<IReadOnlyList<ContactBirthdayResult>> GetContactsBirthdaysAsync(
        [Description("The name or names of contacts we want to retrieve birthdays for.")]
        IReadOnlyList<string> contactNames,
        [Description("If this is a question about who has a birthday in a given month, this is the month being requested. This should be -1 if the question is about a specific contact or contacts birthday. This is should be 0 if the question is about upcoming birthdays.")]
        int month,
        [Description("Describe why you decided to call this function.")]
        string justification,
        CancellationToken cancellationToken)
    {
        // Look up birthdays for specific contacts.
        // Query the database for the contacts(s).
        var contacts = await GetContactsAsync(contactNames, month, cancellationToken);

        return contacts.Select(c => c.ToContactBirthdayResult()).ToList();
    }

    async Task<IReadOnlyList<Contact>> GetContactsAsync(
        IReadOnlyList<string> contactNames,
        int month,
        CancellationToken cancellationToken)
    {
        if (contactNames is not [])
        {
            // Look up birthdays for specific contacts.
            // Query the database for the contacts(s).
            return await db.Contacts
                .Where(c => c.Names.Any(n => contactNames.Any(cn => EF.Functions.ILike(n.UnstructuredName, cn))))
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        if (month is > 0 and <= 12)
        {
            // Look up birthdays for contacts with birthdays in a specific month.
            return await db.Contacts
                .Where(c => c.Birthday!.Month == month)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        if (month == 0)
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

        return [];
    }
}
