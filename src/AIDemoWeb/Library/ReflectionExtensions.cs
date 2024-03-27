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
}
