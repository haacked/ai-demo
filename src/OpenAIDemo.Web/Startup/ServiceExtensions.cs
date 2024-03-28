using System.Text.Json;
using Haack.AIDemoWeb.Entities;
using Haack.AIDemoWeb.Library;
using Haack.AIDemoWeb.Library.Clients;
using MassTransit;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using Refit;
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
                "in `appsettings.Development.json` within `OpenAIDemo.Web`.");
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

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseNetTopologySuite();
        dataSourceBuilder.UseVector();
        var dataSource = dataSourceBuilder.Build();

        options.ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.NavigationBaseIncludeIgnored));

        options.UseNpgsql(dataSource, o =>
        {
            o.UseVector();
            o.UseNetTopologySuite();
            o.MigrationsAssembly("OpenAIDemo.Web");
        });
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

                o.Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        var serviceProvider = context.HttpContext.RequestServices;
                        if (context.Identity is { Name: { } username })
                        {
                            var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<AIDemoContext>>();
                            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
                            var user = await dbContext.Users
                                .FirstOrDefaultAsync(u => u.Name == username);
                            if (user is null)
                            {
                                await dbContext.Users.AddAsync(new User
                                {
                                    Name = username,
                                });
                                await dbContext.SaveChangesAsync();
                            }

                        }
                    }
                };
            });
    }

    /// <summary>
    /// Configure Mass Transit.
    /// </summary>
    /// <param name="services"></param>
    public static void AddMassTransitConfig(this IServiceCollection services)
    {
        services.AddMassTransit(configurator =>
        {
            configurator.AddConsumers(typeof(ServiceExtensions).Assembly);
            configurator.SetKebabCaseEndpointNameFormatter();

            configurator.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });
    }

    public static void AddClients(this IServiceCollection services)
    {
        services.AddTransient<LoggingHttpMessageHandler>();
        services.AddTransient<GeocodeClient>();
        services.AddRefitClient<IOpenAIClient>(
            IOpenAIClient.BaseAddress,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                PropertyNameCaseInsensitive = true,
            });
        services.AddRefitClient<IGoogleGeocodeClient>(
            IGoogleGeocodeClient.BaseAddress,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                PropertyNameCaseInsensitive = true,
            });
        services.AddRefitClient<IWeatherApiClient>(IWeatherApiClient.BaseAddress);
    }

    static IHttpClientBuilder AddRefitClient<T>(this IServiceCollection services, Uri baseAddress) where T : class
    {
        return services.AddRefitClient<T>(new RefitSettings())
            .ConfigureHttpClient(c => c.BaseAddress = baseAddress)
#if DEBUG
            .AddHttpMessageHandler<LoggingHttpMessageHandler>()
#endif
            ;
    }

    static IHttpClientBuilder AddRefitClient<T>(
        this IServiceCollection services,
        Uri baseAddress,
        JsonSerializerOptions serializerOptions) where T : class
    {
        return services.AddRefitClient<T>(new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(serializerOptions),
        })
            .ConfigureHttpClient(c => c.BaseAddress = baseAddress)
#if DEBUG
            .AddHttpMessageHandler<LoggingHttpMessageHandler>()
#endif
            ;
    }
}