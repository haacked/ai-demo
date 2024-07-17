using System.Reflection;

namespace AIDemoWeb.Library;

/// <summary>
/// Useful methods for dealing with reflection and examining types.
/// </summary>
public static class ReflectionExtensions
{
    public static IEnumerable<Type> GetInstantiableTypesThatImplement<TInterface>(
        params Assembly[] assemblies)
    {
        return assemblies.SelectMany(a => a.DefinedTypes)
            .Where(type => type is { IsInterface: false, IsAbstract: false })
            .Where(Implements<TInterface>);
    }

    static bool Implements<TInterface>(this TypeInfo typeInfo)
    {
        return typeof(TInterface).IsAssignableFrom(typeInfo);
    }

    /// <summary>
    /// If this type is a collection (or array), it returns the element type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns>The collection element type, if any.</returns>
    public static Type? GetCollectionElementType(this Type type)
    {
        if (type.IsArray)
            return type.GetElementType();

        return type
            .GetInterfaces()
            .Concat(new[] { type })
            .Select(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                ? i.GetGenericArguments()[0]
                : null)
            .FirstOrDefault(t => t != null);
    }
}
