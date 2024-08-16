using System.Security.Claims;
using System.Text.Json;
using AIDemo.Entities;
using AIDemo.Library.Clients;
using AIDemo.Web.Startup;
using MassTransit;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.EntityFrameworkCore;
using Refit;
using Serious;
using LoggingHttpMessageHandler = AIDemo.Library.Clients.LoggingHttpMessageHandler;

namespace Haack.AIDemoWeb.Startup;

public static class ServiceExtensions
{
    public static IHostApplicationBuilder AddDatabase(this IHostApplicationBuilder builder)
    {
        builder.AddDbInitializationServices<AIDemoDbInitializer, AIDemoDbContext>()
            .AddNpgsqlDbContext<AIDemoDbContext>(
                connectionName: "postgresdb",
                configureDbContextOptions: options => options.UseNpgsql(
                    o =>
                    {
                        o.UseVector();
                        o.UseNetTopologySuite();
                        o.MigrationsAssembly("AIDemo.Web");
                    })
            );
        builder.Services.AddDbContextFactory<AIDemoDbContext>();
        return builder;
    }

    public static IHostApplicationBuilder AddAuthentication(this IHostApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        builder.Services.AddAuthentication(o =>
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
                            var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<AIDemoDbContext>>();
                            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
                            var user = await dbContext.Users
                                .FirstOrDefaultAsync(u => u.NameIdentifier == nameIdentifier);
                            if (user is null)
                            {
                                await dbContext.Users.AddAsync(new User
                                {
                                    NameIdentifier = nameIdentifier,
                                });

                                await dbContext.SaveChangesAsync();
                            }
                        }
                    }
                };
            });

        return builder;
    }

    /// <summary>
    /// Configure Mass Transit.
    /// </summary>
    /// <param name="builder"></param>
    public static IHostApplicationBuilder AddMassTransitConfig(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMassTransit(configurator =>
        {
            configurator.AddConsumers(typeof(ServiceExtensions).Assembly);
            configurator.SetKebabCaseEndpointNameFormatter();

            configurator.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });

        return builder;
    }

    public static IHostApplicationBuilder AddClients(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        services.AddTransient<LoggingHttpMessageHandler>();
        services.AddTransient<GeocodeClient>();
        services.AddTransient<GoogleApiClient>();
        services.AddRefitClient<IGoogleGeocodeClient>(
            IGoogleGeocodeClient.BaseAddress,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                PropertyNameCaseInsensitive = true,
            });
        services.AddRefitClient<IWeatherApiClient>(IWeatherApiClient.BaseAddress);

        return builder;
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