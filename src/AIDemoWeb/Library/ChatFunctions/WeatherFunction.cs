using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.Extensions.Options;

#pragma warning disable CA1707, CA1002, CA2000

namespace Serious.ChatFunctions;

/// <summary>
/// Extends Chat GPT with a function that returns the current weather in a given location.
/// </summary>
public class WeatherFunction : ChatFunction<WeatherArguments, WeatherResult>
{
    readonly IWeatherApiClient _weatherApiClient;
    readonly string _apiKey;

    protected override string Name => "get_current_weather";

    protected override string Description => "Get the current weather in a given location.";

    public int Order => 1; // Comes after the UserFactFunction

    public WeatherFunction(IWeatherApiClient weatherApiClient, IOptions<WeatherOptions> weatherOptions)
    {
        _weatherApiClient = weatherApiClient;
        _apiKey = weatherOptions.Value.ApiKey.Require();
    }

    protected override async Task<WeatherResult?> InvokeAsync(
        WeatherArguments arguments,
        string source,
        CancellationToken cancellationToken)
    {
        var units = arguments.Unit switch
        {
            Unit.Fahrenheit => "imperial",
            Unit.Celsius => "metric",
            _ => "standard"
        };

        var response = await _weatherApiClient.GetWeatherAsync(_apiKey, arguments.Location, units, cancellationToken);

        return response is not null
            ? new WeatherResult(response.Main.Temperature, arguments.Unit)
            : null;
    }
}

/// <summary>
/// The arguments to the weather service.
/// </summary>
/// <param name="Location">The city and state, e.g. San Francisco, CA</param>
/// <param name="Unit">Either "celsius" or "fahrenheit"</param>
public record WeatherArguments(
    [property: Required]
    [property: JsonPropertyName("location")]
    [property: Description("The city and state, e.g. San Francisco, CA")]
    string Location,

    [property: JsonPropertyName("unit")]
    Unit Unit = Unit.Fahrenheit);

public record WeatherResult(double Temperature, Unit Unit);
