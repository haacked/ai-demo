using System.ComponentModel;
using AIDemo.Web.Messages;
using AIDemo.Library.Clients;
using Microsoft.SemanticKernel;

namespace AIDemo.SemanticKernel.Plugins;

public class UnitConverterPlugin
{
    const double KilometersToMiles = 0.621371;
    const double MilesToKilometers = 1.60934;
    const double CelsiusToFahrenheit = 1.8;
    const double FahrenheitToCelsius = 0.5555555555555556;

    [KernelFunction]
    [Description(
        "Converts a distance measurement or set of distance measurements from one unit to another. For example, this can convert kilometers to miles or vice versa. Always use this when the user requests a distance in miles but your result is in kilometers or vice versa.")]
#pragma warning disable CA1822
    public IReadOnlyList<Measurement<DistanceUnit>> ConvertDistances(
#pragma warning restore CA1822
        [Description("The distance measurements to convert.")]
        IReadOnlyList<double> measurements,
        [Description("The unit of the measurements.")]
        DistanceUnit sourceUnit,
        CancellationToken cancellationToken = default)
    {
        return measurements.Select(measurement => sourceUnit switch
        {
            DistanceUnit.Kilometers
                => new Measurement<DistanceUnit>(measurement * KilometersToMiles, DistanceUnit.Miles),
            DistanceUnit.Miles
                => new Measurement<DistanceUnit>(measurement * MilesToKilometers, DistanceUnit.Kilometers),
            _ => throw new ArgumentOutOfRangeException(nameof(sourceUnit), sourceUnit, null)
        }).ToList();
    }

    [KernelFunction]
    [Description(
        "Converts a temperature measurement or set of temperature measurements from one unit to another. For example, this can convert celsiuse to fahrenheit or vice versa. Always use this when the user requests a temperature in fahrenheit but your result is in celsius or vice versa.")]
#pragma warning disable CA1822
    public IReadOnlyList<Measurement<TemperatureUnit>> ConvertTemperatures(
#pragma warning restore CA1822
        [Description("The temperature measurements to convert.")]
        IReadOnlyList<double> measurements,
        [Description("The unit of the measurements.")]
        TemperatureUnit sourceUnit,
        CancellationToken cancellationToken = default)
    {
        return measurements.Select(measurement => sourceUnit switch
        {
            TemperatureUnit.Celsius
                => new Measurement<TemperatureUnit>(measurement * CelsiusToFahrenheit, TemperatureUnit.Fahrenheit),
            TemperatureUnit.Fahrenheit
                => new Measurement<TemperatureUnit>(measurement * FahrenheitToCelsius, TemperatureUnit.Celsius),
            _ => throw new ArgumentOutOfRangeException(nameof(sourceUnit), sourceUnit, null)
        }).ToList();
    }
}

