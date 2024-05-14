using System.Security.Claims;
using System.Text.Json;
using Haack.AIDemoWeb.Entities;
using Haack.AIDemoWeb.Library;
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
            .AddGoogle(o =>
            {
                o.ClientId = configuration["Google:OAuthClientId"].Require();
                o.ClientSecret = configuration["Google:OAuthClientSecret"].Require();
                o.Scope.Add("https://www.googleapis.com/auth/contacts.readonly");
                o.Scope.Add("profile"); // Request access to the user's profile information
                o.SaveTokens = true;
                o.AccessType = "offline"; // Request a refresh token

                o.Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        var identity = context.Identity.Require();

                        identity.AddClaim(new Claim("image", context.User.GetProperty("picture").ToString()));

                        var serviceProvider = context.HttpContext.RequestServices;

                        // Look up or create user based on NameIdentifier.
                        if (identity.FindFirst(ClaimTypes.NameIdentifier)?.Value is { Length: > 0 } nameIdentifier)
                        {
                            var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<AIDemoContext>>();
                            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
                            var user = await dbContext.Users
                                .FirstOrDefaultAsync(u => u.NameIdentifier == nameIdentifier);
                            if (user is null)
                            {
                                user = new User
                                {
                                    NameIdentifier = nameIdentifier,
                                };
                                await dbContext.Users.AddAsync(user);
                            }

                            // Store the refresh token if available
                            var refreshToken = context.RefreshToken;
                            if (!string.IsNullOrEmpty(refreshToken))
                            {
                                user.RefreshToken = refreshToken;
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
        services.AddTransient<GoogleApiClient>();
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
