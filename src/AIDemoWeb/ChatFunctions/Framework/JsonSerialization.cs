using System.Text.Json;
using System.Text.Json.Serialization;

namespace Serious.ChatFunctions;

public static class JsonSerialization
{
    public static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new JsonStringEnumConverter() },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}