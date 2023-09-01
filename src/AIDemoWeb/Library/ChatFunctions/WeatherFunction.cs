using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Web;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.Extensions.Options;

#pragma warning disable CA1707, CA1002, CA2000

namespace Serious.ChatFunctions;

/// <summary>
/// Extends Chat GPT with a function that returns the current weather in a given location.
/// </summary>
public class WeatherChatFunction : ChatFunction<WeatherArguments, WeatherResult>
{
    readonly string _apiKey;

    protected override string Name => "get_current_weather";

    protected override string Description => "Get the current weather in a given location.";


    public WeatherChatFunction(IOptions<WeatherOptions> weatherOptions)
    {
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
        var apiKey = HttpUtility.UrlEncode(_apiKey);
        var location = HttpUtility.UrlEncode(arguments.Location);
        var weatherEndpoint = $"https://api.openweathermap.org/data/2.5/weather?appid={apiKey}&q={location}&units={units}";
        var httpClient = new HttpClient();
        var response = await httpClient.GetFromJsonAsync<WeatherResponse>(new Uri(weatherEndpoint), cancellationToken);

        return response is not null
            ? new WeatherResult(response.Main.Temperature, arguments.Unit)
            : null;
    }
}

public record WeatherResult(double Temperature, Unit Unit);

public enum Unit
{
    [EnumMember(Value = "celsius")]
    Celsius,

    [EnumMember(Value = "fahrenheit")]
    Fahrenheit,
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

public record Coordinates(
    [property: JsonPropertyName("lon")]
    double Longitude,

    [property: JsonPropertyName("lat")]
    double Latitude);

public record Weather(
    [property: JsonPropertyName("id")]
    int Id,

    [property: JsonPropertyName("main")]
    string Main,

    [property: JsonPropertyName("description")]
    string Description,

    [property: JsonPropertyName("icon")]
    string Icon);

public record MainWeatherData(
    [property: JsonPropertyName("temp")]
    double Temperature,

    [property: JsonPropertyName("feels_like")]
    double FeelsLike,

    [property: JsonPropertyName("temp_min")]
    double MinTemperature,

    [property: JsonPropertyName("temp_max")]
    double MaxTemperature,

    [property: JsonPropertyName("pressure")]
    int Pressure,

    [property: JsonPropertyName("humidity")]
    int Humidity,

    [property: JsonPropertyName("sea_level")]
    int SeaLevel,

    [property: JsonPropertyName("grnd_level")]
    int GroundLevel);

public record Wind(
    [property: JsonPropertyName("speed")]
    double Speed,

    [property: JsonPropertyName("deg")]
    int Degree,

    [property: JsonPropertyName("gust")]
    double Gust);

public record Clouds(
    [property: JsonPropertyName("all")]int All);

public record Sys(
    [property: JsonPropertyName("type")]
    int Type,

    [property: JsonPropertyName("id")]
    int Id,

    [property: JsonPropertyName("country")]
    string Country,

    [property: JsonPropertyName("sunrise")]
    int Sunrise,

    [property: JsonPropertyName("sunset")]
    int Sunset);

public record WeatherResponse(

    [property: JsonPropertyName("coord")]
    Coordinates Coordinates,

    [property: JsonPropertyName("weather")]
    List<Weather> Weather,

    [property: JsonPropertyName("base")]
    string Base,

    [property: JsonPropertyName("main")]
    MainWeatherData Main,

    [property: JsonPropertyName("visibility")]
    int Visibility,

    [property: JsonPropertyName("wind")]
    Wind Wind,

    [property: JsonPropertyName("clouds")]
    Clouds Clouds,

    [property: JsonPropertyName("dt")]
    long Dt,

    [property: JsonPropertyName("sys")]
    Sys Sys,

    [property: JsonPropertyName("timezone")]
    int Timezone,

    [property: JsonPropertyName("id")]
    int Id,

    [property: JsonPropertyName("name")]
    string Name,

    [property: JsonPropertyName("cod")]
    int Cod
);