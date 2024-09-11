using System.Security.Claims;
using System.Text.Json;
using AIDemo.Hubs;
using AIDemo.Web.Startup;
using Azure.AI.OpenAI;
using AIDemo.Entities;
using AIDemo.Library.Clients;
using AIDemo.SemanticKernel.Plugins;
using Haack.AIDemoWeb.SemanticKernel.Plugins;
using Haack.AIDemoWeb.Startup.Config;
using MassTransit;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Logging;
using Microsoft.SemanticKernel;
using Refit;
using Serious;
using Serious.ChatFunctions;
using LoggingHttpMessageHandler = AIDemo.Library.Clients.LoggingHttpMessageHandler;

namespace Haack.AIDemoWeb.Startup;

public static class ServiceExtensions
{
    public static IHostApplicationBuilder AddSemanticKernel(this IHostApplicationBuilder builder)
    {
        var options = builder.GetConfigurationSection<OpenAIOptions>().Require();

        builder.Services.AddOpenAIChatCompletion(
            options.Model,
            options.ApiKey.Require())
        // Ugh, I think it's dumb I have to pass an OpenAIClient here. I hope they fix that up soon.
        .AddOpenAITextEmbeddingGeneration(modelId: options.EmbeddingModel, openAIClient: new OpenAIClient(options.ApiKey));

        builder.Services
            .AddTransient<ContactFactsPlugin>()
            .AddTransient<ContactPlugin>()
            .AddTransient<WeatherPlugin>()
            .AddTransient<UnitConverterPlugin>()
            .AddTransient<LocationPlugin>();

        builder.Services.AddTransient<Kernel>(serviceProvider =>
        {
            var kernel = new Kernel(serviceProvider);

            var filter = new FunctionSignalFilter(serviceProvider.GetRequiredService<IHubContext<BotHub>>());
            kernel.FunctionInvocationFilters.Add(filter);
            kernel.AutoFunctionInvocationFilters.Add(filter);
#pragma warning disable CS0618 // Type or member is obsolete
            kernel.FunctionInvoked += (_, args) =>
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                filter.OnFunctionInvokedAsync(args);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            };

            kernel.ImportPluginFromType<ContactFactsPlugin>();
            kernel.ImportPluginFromType<ContactPlugin>();
            kernel.ImportPluginFromType<UnitConverterPlugin>();
            kernel.ImportPluginFromType<WeatherPlugin>();
            kernel.ImportPluginFromType<LocationPlugin>();

            return kernel;
        });

        return builder;
    }

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