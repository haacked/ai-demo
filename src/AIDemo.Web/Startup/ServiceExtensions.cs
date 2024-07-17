using System.Security.Claims;
using System.Text.Json;
using Haack.AIDemoWeb.Entities;
using Haack.AIDemoWeb.Library;
using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Plugins;
using Haack.AIDemoWeb.SemanticKernel.Plugins;
using Haack.AIDemoWeb.Startup.Config;
using MassTransit;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.SemanticKernel;
using Npgsql;
using OpenAIDemo.Hubs;
using Refit;
using Serious;
using Serious.ChatFunctions;

namespace Haack.AIDemoWeb.Startup;

public static class ServiceExtensions
{
    public static IServiceCollection AddSemanticKernel(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(OpenAIOptions.OpenAI).Get<OpenAIOptions>().Require();

        services.AddOpenAIChatCompletion(
            options.Model,
            options.ApiKey.Require());

        services
            .AddTransient<ContactFactsPlugin>()
            .AddTransient<ContactPlugin>()
            .AddTransient<WeatherPlugin>()
            .AddTransient<UnitConverterPlugin>()
            .AddTransient<LocationPlugin>();

        services.AddTransient<Kernel>(serviceProvider =>
        {
            var kernel = new Kernel(serviceProvider);

#pragma warning disable SKEXP0001
            var filter = new FunctionSignalFilter(serviceProvider.GetRequiredService<IHubContext<BotHub>>());
            kernel.FunctionInvocationFilters.Add(filter);
            kernel.AutoFunctionInvocationFilters.Add(filter);
#pragma warning disable CS0618 // Type or member is obsolete
            kernel.FunctionInvoked += (sender, args) =>
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                filter.OnFunctionInvokedAsync(args);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            };

#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore SKEXP0001

            kernel.ImportPluginFromType<ContactFactsPlugin>();
            kernel.ImportPluginFromType<ContactPlugin>();
            kernel.ImportPluginFromType<UnitConverterPlugin>();
            kernel.ImportPluginFromType<WeatherPlugin>();
            kernel.ImportPluginFromType<LocationPlugin>();

            return kernel;
        });

        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(AIDemoContext.ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"The `ConnectionStrings:AIDemoContext` setting is not configured in the `ConnectionStrings`" +
                $" section. For local development, make sure `ConnectionStrings:AIDemoContext` is set properly " +
                "in `appsettings.Development.json` within `AIDemo.Web`.");
        services.AddDbContextFactory<AIDemoContext>(
            options => SetupDbContextOptions(connectionString, options));
        services.AddDbContext<AIDemoContext>(
            options => SetupDbContextOptions(connectionString, options),
            optionsLifetime: ServiceLifetime.Singleton);

        return services;
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
            o.MigrationsAssembly("AIDemo.Web");
        });
    }

    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
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

        return services;
    }

    /// <summary>
    /// Configure Mass Transit.
    /// </summary>
    /// <param name="services"></param>
    public static IServiceCollection AddMassTransitConfig(this IServiceCollection services)
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

        return services;
    }

    public static IServiceCollection AddClients(this IServiceCollection services)
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

        return services;
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