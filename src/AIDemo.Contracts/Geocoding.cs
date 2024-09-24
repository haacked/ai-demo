namespace AIDemo.Library.Clients;

public record GoogleGeoCodingResponse(IReadOnlyList<GoogleGeoCodeResult> Results, string Status);

public record GoogleGeoCodeResult(
    IReadOnlyList<GoogleAddressComponent> AddressComponents,
    string FormattedAddress,
    GoogleGeometry Geometry,
    string PlaceId,
    IReadOnlyList<string> Types);

public record GoogleAddressComponent(string LongName, string ShortName, IReadOnlyList<string> Types);

public record GoogleGeometry(
    GoogleBounds Bounds,
    GoogleLocation Location,
    string LocationType,
    GoogleViewport Viewport);

public record GoogleBounds(GoogleLocation Northeast, GoogleLocation Southwest);

public record GoogleLocation(double Lat, double Lng);

public record GoogleViewport(GoogleLocation Northeast, GoogleLocation Southwest);

public record GoogleTimeZoneResponse(int DstOffset, int RawOffset, string Status, string TimeZoneId, string TimeZoneName);
