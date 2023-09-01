using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
#pragma warning disable CA1308

namespace Serious.ChatFunctions;

public static class BinaryDataGenerator
{
    public static BinaryData GenerateBinaryData(Type type)
    {
        var properties = new Dictionary<string, object>();

        foreach (var propertyInfo in type.GetProperties())
        {
            var propertyName = propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? propertyInfo.Name;
            var propertyType = GetPropertyType(propertyInfo);

            var propertyData = new Dictionary<string, object>
            {
                { "Type", propertyType.Name }
            };

            if (propertyType.IsEnum)
            {
                propertyData["Enum"] = Enum.GetNames(propertyType);
            }

            var propertyDescription = propertyInfo.GetCustomAttribute<DescriptionAttribute>()?.Description;
            if (!string.IsNullOrEmpty(propertyDescription))
            {
                propertyData["Description"] = propertyDescription;
            }

            properties[propertyName] = propertyData;
        }

        var requiredProperties = type.GetProperties()
            .Where(p => p.GetCustomAttribute<RequiredAttribute>() != null)
            .Select(p => p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? p.Name)
            .ToArray();

        var result = new Dictionary<string, object>
        {
            { "Type", "object" },
            { "Properties", properties },
            { "Required", requiredProperties }
        };

        return BinaryData.FromObjectAsJson(result, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        });
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
}