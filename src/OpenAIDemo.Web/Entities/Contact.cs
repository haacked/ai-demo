using System.ComponentModel.DataAnnotations.Schema;
using Google.Apis.PeopleService.v1.Data;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Haack.AIDemoWeb.Entities;

#pragma warning disable CA1002

public class Contact
{
    public int Id { get; set; }

    /// <summary>
    /// The resource name for the person, assigned by the server. An ASCII string in the form of people/{person_id}.
    /// </summary>
    public required string ResourceName { get; set; }

    public List<ContactName> Names { get; init; } = new();

    public List<ContactEmailAddress> EmailAddresses { get; init; } = new();

    public List<ContactAddress> Addresses { get; init; } = new();
}

public record ContactName(
    string UnstructuredName,
    string? FamilyName,
    string? GivenName,
    string? MiddleName,
    string? HonorificPrefix,
    string? HonorificSuffix,
    string? PhoneticFullname,
    string? PhoneticFamilyName,
    string? PhoneticGivenName,
    string? PhoneticMiddleName,
    string? PhoneticHonorificPrefix,
    string? PhoneticHonorificSuffix)
{
    public static ContactName FromGoogleContactName(Name name) =>
        new(
            name.UnstructuredName,
            name.FamilyName,
            name.GivenName,
            name.MiddleName,
            name.HonorificPrefix,
            name.HonorificSuffix,
            name.PhoneticFullName,
            name.PhoneticFamilyName,
            name.PhoneticGivenName,
            name.PhoneticMiddleName,
            name.PhoneticHonorificPrefix,
            name.PhoneticHonorificSuffix);
};

[Owned]
public record ContactEmailAddress(string? Value, string? Type, string? DisplayName)
{
    public static ContactEmailAddress FromGoogleContactEmail(EmailAddress email) =>
        new(email.Value, email.Type, email.DisplayName);
};

[Owned]
public record ContactAddress(
    string? FormattedValue,
    string? Type,
    string? PoBox,
    string? StreetAddress,
    string? ExtendedAddress,
    string? City,
    string? Region,
    string? PostalCode,
    string? Country,
    string? CountryCode,
    [property: Column(TypeName = "geometry (point)")]
    Point? Location)
{
    public static ContactAddress FromGoogleContactAddress(Address address, Point? location) =>
        new(
            address.FormattedValue,
            address.Type,
            address.PoBox,
            address.StreetAddress,
            address.ExtendedAddress,
            address.City,
            address.Region,
            address.PostalCode,
            address.Country,
            address.CountryCode,
            location);
};

