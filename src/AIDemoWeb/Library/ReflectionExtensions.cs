using System.Numerics;
using System.Reflection;

namespace AIDemoWeb.Library;

/// <summary>
/// Useful methods for dealing with reflection and examining types.
/// </summary>
public static class ReflectionExtensions
{
    /// <summary>
    /// Used to call a non-public constructor of a type.
    /// </summary>
    /// <param name="args">The arguments used to look up and pass to the ctor.</param>
    /// <typeparam name="T">The target type.</typeparam>
    /// <returns>An instance of the type.</returns>
    /// <exception cref="ArgumentException">If the passed args don't match anything.</exception>
    public static T Instantiate<T>(params object[] args)
    {
        var type = typeof(T);
        Type[] parameterTypes = args.Select(p => p.GetType()).ToArray();
        var constructor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, parameterTypes, null);

        if (constructor is null)
        {
            throw new ArgumentException("The args don't match any ctor");
        }

        return (T)constructor.Invoke(args);
    }

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

    public static bool Implements<TInterface>(this Type type)
    {
        return typeof(TInterface).IsAssignableFrom(type);
    }

    public static Version GetAssemblyVersion(Type? type = null)
    {
        var assembly = type?.Assembly ?? Assembly.GetEntryAssembly() ?? typeof(ReflectionExtensions).Assembly;
        var version = assembly.GetName().Version;
        if (version is null)
        {
            throw new InvalidOperationException($"Assembly {assembly.FullName} has a null version.");
        }
        return version;
    }

    /// <summary>
    /// Returns true if the type has the specified attribute applied.
    /// </summary>
    /// <typeparam name="TAttribute">The attribute to look for.</typeparam>
    public static bool HasAttribute<TAttribute>(this Type type) where TAttribute : Attribute
    {
        var attribute = Attribute.GetCustomAttribute(type, typeof(TAttribute));
        return attribute is not null;
    }

    /// <summary>
    /// If the type is a <see cref="Nullable{T}"/>, this returns the type T, otherwise it returns the passed in type.
    /// </summary>
    /// <param name="type">The type to test.</param>
    public static Type GetUnderlyingNullableTypeOrType(this Type type)
    {
        return Nullable.GetUnderlyingType(type) ?? type;
    }

    /// <summary>
    /// Given a CLR <see cref="Type"/>, this method attempts to return the closest JavaScript primitive type. For
    /// anything it doesn't understand, it'll return "Object".
    /// </summary>
    /// <param name="type">The CLR type</param>
    public static string GetJavaScriptType(this Type type)
    {
        // TODO: Look into "TimeSpan" support.
        var underlyingType = GetUnderlyingNullableTypeOrType(type);
        return underlyingType == typeof(bool)
            ? "Boolean"
            : underlyingType == typeof(DateTime) || underlyingType == typeof(DateTimeOffset)
                ? "Date"
                : underlyingType == typeof(string)
                    ? "String"
                    : underlyingType == typeof(BigInteger)
                        ? "BigInt"
                        : underlyingType == typeof(byte)
                          || underlyingType == typeof(short)
                          || underlyingType == typeof(ushort)
                          || underlyingType == typeof(int)
                          || underlyingType == typeof(uint)
                          || underlyingType == typeof(nint)
                          || underlyingType == typeof(nuint)
                          || underlyingType == typeof(ulong)
                          || underlyingType == typeof(sbyte)
                          || underlyingType == typeof(long)
                          || underlyingType == typeof(float)
                          || underlyingType == typeof(double)
                          || underlyingType == typeof(decimal)
                          || underlyingType == typeof(Half)
                        ? "Number"
                        : "Object";
    }
}
