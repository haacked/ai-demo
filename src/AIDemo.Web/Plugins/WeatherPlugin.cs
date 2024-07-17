using System.ComponentModel;
using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace Serious.ChatFunctions;

/// <summary>
/// Extends Chat GPT with a function that returns the current weather in a given location.
/// </summary>
public class WeatherPlugin(IWeatherApiClient weatherApiClient, IOptions<WeatherOptions> weatherOptions)
{
    readonly string _apiKey = weatherOptions.Value.ApiKey.Require();

    [KernelFunction]
    [Description("Get the current weather in a given location. If given a city and state, use the full state name.")]
    public async Task<WeatherResult?> GetWeatherAsync(
        [Description("The country or city and state, e.g. San Francisco, California to get the weather for")]
        string location,
        [Description("The unit of temperature if specified.")]
        TemperatureUnit unit = TemperatureUnit.Fahrenheit,
        CancellationToken cancellationToken = default)
    {
        var units = unit switch
        {
            TemperatureUnit.Fahrenheit => "imperial",
            TemperatureUnit.Celsius => "metric",
            _ => "standard"
        };

        var response = await weatherApiClient.GetWeatherAsync(_apiKey, location, units, cancellationToken);

        return response is not null
            ? new WeatherResult(response.Main.Temperature, unit)
            : null;
    }
}

public record WeatherResult(double Temperature, TemperatureUnit TemperatureUnit);