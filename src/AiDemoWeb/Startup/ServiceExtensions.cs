using Haack.AIDemoWeb.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Serious;

namespace Haack.AIDemoWeb.Startup;

public static class ServiceExtensions
{
    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
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

    public static void AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(o =>
            {
                o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(o =>
            {
                // set the path for the authentication challenge
                o.LoginPath = "/Login";
                // set the path for the sign out
                o.LogoutPath = "/Logout";
            })
            .AddGitHub(o =>
            {
                o.ClientId = configuration["GitHub:ClientId"].Require();
                o.ClientSecret = configuration["GitHub:ClientSecret"].Require();
                o.CallbackPath = "/signin-github";

                // Grants access to read a user's profile data.
                // https://docs.github.com/en/developers/apps/building-oauth-apps/scopes-for-oauth-apps
                o.Scope.Add("read:user");
            });
    }
}