using System.Text.Json.Serialization;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.Extensions.Options;

#pragma warning disable CA1707, CA1002, CA2000

namespace Serious.ChatFunctions;

public class WeatherFunction
{
    readonly string _apiKey;

    public WeatherFunction(IOptions<WeatherOptions> weatherOptions)
    {
        _apiKey = weatherOptions.Value.ApiKey.Require();
    }

    public async Task<WeatherResult?> GetWeatherAsync(WeatherArguments weatherArguments, CancellationToken cancellationToken)
    {
        var units = weatherArguments.Unit switch
        {
            "fahrenheit" => "imperial",
            "celsius" => "metric",
            _ => "standard"
        };
        var weatherEndpoint = $"https://api.openweathermap.org/data/2.5/weather?appid={_apiKey}&q={weatherArguments.Location}&units={units}";
        var httpClient = new HttpClient();
        var response = await httpClient.GetFromJsonAsync<WeatherResponse>(new Uri(weatherEndpoint), cancellationToken);
        return response is not null
            ? new WeatherResult(response.Main.Temperature, units)
            : null;
    }
}

public record WeatherResult(double Temperature, string Unit);

public record WeatherArguments(
    [property: JsonPropertyName("location")]
    string Location,

    [property: JsonPropertyName("unit")]
    string? Unit = "fahrenheit");

public record Coord(
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
    Coord Coordinates,

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