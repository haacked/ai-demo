namespace Haack.AIDemoWeb.Library;

using System;

public static class DistanceCalculator
{
    // Radius of the Earth in kilometers
    const double EarthRadiusKm = 6371;

    public static double CalculateDistance(Coordinate firstCoordinate, Coordinate secondCoordinate)
    {
        return CalculateDistance(
            firstCoordinate.Latitude,
            firstCoordinate.Longitude,
            secondCoordinate.Latitude,
            secondCoordinate.Longitude);
    }

    // Method to calculate distance between two points in kilometers using Haversine formula
    public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Convert latitude and longitude from degrees to radians
        var lat1Rad = ToRadians(lat1);
        var lon1Rad = ToRadians(lon1);
        var lat2Rad = ToRadians(lat2);
        var lon2Rad = ToRadians(lon2);

        // Calculate differences in latitude and longitude
        var deltaLat = lat2Rad - lat1Rad;
        var deltaLon = lon2Rad - lon1Rad;

        // Haversine formula
        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        // Calculate the distance
        var distance = EarthRadiusKm * c;

        return distance;
    }

    // Helper method to convert degrees to radians
    static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
}
