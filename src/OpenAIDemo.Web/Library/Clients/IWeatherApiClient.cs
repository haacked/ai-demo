using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Refit;

namespace Haack.AIDemoWeb.Library.Clients;

public interface IWeatherApiClient
{
    public static Uri BaseAddress => new("https://api.openweathermap.org/data/2.5/");

    [Get("/weather?appid={apiKey}&q={location}&units={units}")]
    Task<WeatherResponse?> GetWeatherAsync(
        string apiKey,
        string location,
        string units,
        CancellationToken cancellationToken);
}


public enum Unit
{
    [EnumMember(Value = "celsius")]
    Celsius,

    [EnumMember(Value = "fahrenheit")]
    Fahrenheit,
}

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
    [property: JsonPropertyName("all")] int All);

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
#pragma warning disable CA1002
    List<Weather> Weather,
#pragma warning restore CA1002

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
