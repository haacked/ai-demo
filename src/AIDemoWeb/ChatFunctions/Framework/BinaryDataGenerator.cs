using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    {
        var properties = new Dictionary<string, object>();

        foreach (var propertyInfo in type.GetProperties())
        {
            var propertyName = propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? propertyInfo.Name;
            var propertyType = GetPropertyType(propertyInfo);

            var propertyData = new Dictionary<string, object>
            {
                { "type", GetGptType(propertyType) }
            };

            if (propertyType.IsEnum)
            {
                propertyData["enum"] = Enum.GetNames(propertyType);
            }

            var propertyDescription = propertyInfo.GetCustomAttribute<DescriptionAttribute>()?.Description;
            if (!string.IsNullOrEmpty(propertyDescription))
            {
                propertyData["description"] = propertyDescription;
            }

            properties[propertyName] = propertyData;
        }

        var requiredProperties = type.GetProperties()
            .Where(p => p.GetCustomAttribute<RequiredAttribute>() != null)
            .Select(p => p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? p.Name)
            .ToArray();

        return new Dictionary<string, object>
        {
            { "type", "object" },
            { "properties", properties },
            { "required", requiredProperties }
        };
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

    static string GetGptType(Type propertyType)
    {
        return propertyType == typeof(int) || propertyType == typeof(double) ? "int" : "string";
    }
}

