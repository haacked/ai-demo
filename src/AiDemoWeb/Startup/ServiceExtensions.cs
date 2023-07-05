using Haack.AIDemoWeb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Haack.AIDemoWeb.Startup;

public static class ServiceExtensions
{
    public static void AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(AIDemoContext.ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"The `ConnectionStrings:AIDemoContext` setting is not configured in the `ConnectionStrings`" +
                $" section. For local development, make sure `ConnectionStrings:AIDemoContext` is set properly " +
                "in `appsettings.Development.json` within `AIDemoWeb`.");
        services.AddDbContextFactory<AIDemoContext>(
            options => SetupDbContextOptions(connectionString, options));
        services.AddDbContext<AIDemoContext>(
            options => SetupDbContextOptions(connectionString, options),
            optionsLifetime: ServiceLifetime.Singleton);

    }

    /// <summary>
    /// Setup a DbContextOptions object for a requested `DbContext` subclass.
    /// </summary>
    /// <param name="connectionString">The connection string to use for the DbContext.</param>
    /// <param name="options"></param>
    /// <exception cref="InvalidOperationException"></exception>
    static void SetupDbContextOptions(string connectionString, DbContextOptionsBuilder options)
    {
#if DEBUG
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
#endif
        options.ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.NavigationBaseIncludeIgnored));

        options.UseNpgsql(connectionString, o => o.MigrationsAssembly("AIDemoWeb"));
    }
}