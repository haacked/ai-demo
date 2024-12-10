using System.Reflection;
using AIDemoWeb.Library;

namespace AIDemoWeb;

internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all types that implement <typeparamref name="T"/> in the specified assemblies.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to register the types in.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/> to register the types with.</param>
    /// <param name="publicOnly">If <c>true</c> only include public types.</param>
    /// <param name="assemblies">The <see cref="Assembly"/>s to scan.</param>
    /// <typeparam name="T">The service interface.</typeparam>
    public static void RegisterAllTypes<T>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Transient,
        bool publicOnly = false,
        params Assembly[] assemblies)
    {
        var typesFromAssemblies = ReflectionExtensions
            .GetInstantiableTypesThatImplement<T>(assemblies)
            .Where(t => !publicOnly || t.IsPublic);

        foreach (var type in typesFromAssemblies)
            services.Add(new ServiceDescriptor(typeof(T), type, lifetime));
    }

    /// <summary>
    /// Registers all types that implement <typeparamref name="TInterface"/> in the same assembly as <typeparamref name="TInterface"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to register the types in.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/> to register the types with.</param>
    /// <param name="publicOnly">If <c>true</c> only include public types.</param>
    /// <typeparam name="TInterface">The service interface.</typeparam>
    public static void RegisterAllTypesInSameAssembly<TInterface>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Transient,
        bool publicOnly = false)
    {
        services.RegisterAllTypes<TInterface>(lifetime, publicOnly, typeof(TInterface).Assembly);
    }
}