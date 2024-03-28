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

    public static IReadOnlyDictionary<string, object> GetParametersDictionary(Type type)
        => GetParametersDictionaryCore(type, null, new HashSet<Type>());

    public static IReadOnlyDictionary<string, object> GetParametersDictionaryCore(
        Type type,
        string? description,
        HashSet<Type> seenComplexTypes)
    {
        var properties = new Dictionary<string, object>();
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

        // If we've already seen this complex type, we don't want to recurse infinitely.
        if (!seenComplexTypes.Add(type))
        {
            return properties;
        }

        foreach (var propertyInfo in type.GetProperties())
        {
            var propertyName = propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? propertyInfo.Name;
            var propertyType = GetPropertyType(propertyInfo);
            var gptType = GetGptType(propertyType);
            var propertyDescription = propertyInfo.GetCustomAttribute<DescriptionAttribute>()?.Description;

            if (gptType.Type == "object")
            {
                properties[propertyName] = GetParametersDictionaryCore(propertyType, propertyDescription, seenComplexTypes);
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
                var subType = GetGptType(gptType.ElementType);

                if (subType.Type != "object")
                {
                    propertyData["items"] = new Dictionary<string, object>
                    {
                        { "type", GetGptType(gptType.ElementType).Type }
                    };
                }
                else
                {
                    propertyData["items"] = GetParametersDictionaryCore(gptType.ElementType, null, seenComplexTypes);
                }
            }

            properties[propertyName] = propertyData;
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
            return new GptType("integer", null);
        }

        if (propertyType == typeof(string) || propertyType.IsEnum)
        {
            return new GptType("string", null);
        }

        var elementType = propertyType.GetCollectionElementType();
        return elementType is not null
            ? new GptType("array", elementType)
            : new GptType("object", null);
    }
}

public record GptType(string Type, Type? ElementType);

