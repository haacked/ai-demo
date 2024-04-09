using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Serious.ChatFunctions;

public class UnitConverterFunction : ChatFunction<ConverterArguments, ConverterResults>
{
    const double KilometersToMiles = 0.621371;
    const double MilesToKilometers = 1.60934;

    protected override string Name => "unit_converter";

    protected override string Description =>
        "Converts a measurement or set of measurements from one unit to another. For example, this can convert kilometers to miles. Always use this when the user requests a measurement in miles but your result is in kilometers or vice versa.";

    protected override Task<ConverterResults?> InvokeAsync(
        ConverterArguments arguments,
        string source,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<ConverterResults?>(new ConverterResults(ConvertMeasurements(arguments).ToList()));
    }

    static IEnumerable<Measurement> ConvertMeasurements(ConverterArguments arguments)
    {
        foreach (var conversion in arguments.Conversions)
        {
            yield return conversion.SourceUnit switch
            {
                DistanceUnit.Kilometers when conversion.TargetUnit == DistanceUnit.Miles
                    => new Measurement(conversion.Value * KilometersToMiles, DistanceUnit.Miles),
                DistanceUnit.Miles when conversion.TargetUnit == DistanceUnit.Kilometers
                    => new Measurement(conversion.Value * MilesToKilometers, DistanceUnit.Kilometers),
                _ => new Measurement(conversion.Value, conversion.SourceUnit)
            };
        }
    }
}

public record ConverterArguments(
    [property: Required]
    [property: JsonPropertyName("conversions")]
    [property: Description("A conversion to make.")]
    IReadOnlyList<Conversion> Conversions);

public record Conversion(
    [property: Required]
    [property: JsonPropertyName("value")]
    [property: Description("The measurement value.")]
    double Value,

    [property: Required]
    [property: JsonPropertyName("source_unit")]
    [property: Description("The unit of the value.")]
    DistanceUnit SourceUnit,

    [property: Required]
    [property: JsonPropertyName("target_unit")]
    [property: Description("The unit to convert the value to.")]
    DistanceUnit TargetUnit);

public record ConverterResults(
    [property: JsonPropertyName("results")]
    IReadOnlyList<Measurement> Results);

public record Measurement(
    [property: JsonPropertyName("value")]
    double Value,

    [property: JsonPropertyName("unit")]
    DistanceUnit Unit);

public enum DistanceUnit
{
    Kilometers,
    Miles,
}