using System.ComponentModel;
using System.Globalization;
using System.Text.Json.Serialization;
using Google.Apis.PeopleService.v1.Data;
using Microsoft.EntityFrameworkCore;

namespace AIDemo.Web.Messages;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DistanceUnit
{
    [Description("The distance in kilometers.")]
    Kilometers,

    [Description("The distance in miles.")]
    Miles,
}

public record Measurement<TUnit>(
    [property: JsonPropertyName("value")]
    double Value,

    [property: JsonPropertyName("unit")]
    TUnit Unit);

public record ContactDistance(
    [property: JsonPropertyName("name")]
    string Name,

    [property: JsonPropertyName("distance")]
    Measurement<DistanceUnit> Distance);

public record Coordinate(
    [property: JsonPropertyName("latitude")]
    double Latitude,

    [property: JsonPropertyName("longitude")]
    double Longitude);

public record ContactLocation(
    [property: JsonPropertyName("coordinate")]
    Coordinate Coordinate,

    [property: JsonPropertyName("formatted_address")]
    string FormattedAddress);

[Owned]
public record ContactBirthday(int Year, int Month, int Day)
{
    public static ContactBirthday? FromGoogleContactBirthday(Birthday birthday) =>
        birthday.Date is { } birthDate
            ? new(birthDate.Year ?? 0, birthDate.Month ?? 0, birthDate.Day ?? 0)
            : null;

    public override string ToString() =>
        (Year, Month, Day) switch
        {
            (_, 0, 0) => "Unknown",
            (0, _, 0) => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Month), // Get month name from Month number
            (0, _, _) => $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Month)} {Day}",
            _ => $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Month)} {Day} {Year}",
        };
};