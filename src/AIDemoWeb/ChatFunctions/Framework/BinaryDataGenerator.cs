using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using AIDemoWeb.Library;

#pragma warning disable CA1308

namespace Serious.ChatFunctions;

public static class BinaryDataGenerator
{
    public static BinaryData ToBinaryData(this IReadOnlyDictionary<string, object> parameters)
    {
        return BinaryData.FromObjectAsJson(parameters, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        });
    }

    public static IReadOnlyDictionary<string, object> GetParametersDictionary(Type type, string? description = null)
    {
        var properties = new Dictionary<string, object>();

        foreach (var propertyInfo in type.GetProperties())
        {
            var propertyName = propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? propertyInfo.Name;
            var propertyType = GetPropertyType(propertyInfo);
            var gptType = GetGptType(propertyType);
            var propertyDescription = propertyInfo.GetCustomAttribute<DescriptionAttribute>()?.Description;

            if (gptType.Type == "object")
            {
                properties[propertyName] = GetParametersDictionary(propertyType, propertyDescription);
                continue;
            }

            var propertyData = new Dictionary<string, object>
            {
                { "type", gptType.Type }
            };

            if (propertyType.IsEnum)
            {
                propertyData["enum"] = Enum.GetNames(propertyType);
            }

            if (propertyDescription is not null or [])
            {
                propertyData["description"] = propertyDescription;
            }

            if (gptType.ElementType is not null)
            {
                propertyData["items"] = new Dictionary<string, object>
                {
                    { "type", GetGptType(gptType.ElementType).Type }
                };
            }

            properties[propertyName] = propertyData;
        }

        var requiredProperties = type.GetProperties()
            .Where(p => p.GetCustomAttribute<RequiredAttribute>() != null)
            .Select(p => p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? p.Name)
            .ToArray();

        var result = new Dictionary<string, object>
        {
            { "type", "object" },
            { "properties", properties },
            { "required", requiredProperties }
        };

        if (description is not null or [])
        {
            result["description"] = description;
        }

        return result;
    }

    static Type GetPropertyType(PropertyInfo propertyInfo)
    {
        if (propertyInfo.PropertyType.IsGenericType &&
            propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return propertyInfo.PropertyType.GetGenericArguments()[0];
        }

        return propertyInfo.PropertyType;
    }

    static GptType GetGptType(Type propertyType)
    {
        if (propertyType == typeof(int) || propertyType == typeof(double))
        {
            return new GptType("int", null);
        }

        if (propertyType == typeof(string) || propertyType.IsEnum)
        {
            return new GptType("string", null);
        }

        var elementType = propertyType.GetCollectionElementType();
        if (elementType is not null)
        {
            return new GptType("array", elementType);
        }

        return new GptType("object", null);
    }
}

public record GptType(string Type, Type? ElementType);

