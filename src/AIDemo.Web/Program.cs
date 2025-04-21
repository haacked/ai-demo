using AIDemo.Blazor.Components;
using AIDemo.Blazor.Library;
using AIDemo.Hubs;
using AIDemo.Library.Clients;
using Haack.AIDemoWeb.Startup;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;

var builder = WebApplication.CreateBuilder(args);

// Create an MCPClient for the GitHub server
await using var gitHubMcpClient = await McpClientFactory.CreateAsync(new StdioClientTransport(new()
{
    Name = "GitHub",
    Command = "npx",
    Arguments = ["-y", "@modelcontextprotocol/server-github"],
}));

// Create an MCPClient for the Bluesky server
await using var blueSky = await McpClientFactory.CreateAsync(new StdioClientTransport(new()
{
    Name = "BlueSky",
    Command = "npx",
    Arguments = ["-y", "mcp-server-bluesky"]
}));

// Retrieve the list of tools available on the GitHub server
var gitHubTools = await gitHubMcpClient.ListToolsAsync().ConfigureAwait(false);
var blueSkyTools = await blueSky.ListToolsAsync().ConfigureAwait(false);
// Add services to the container.
builder
    .AddServiceDefaults()
    .AddClients()
    .AddDatabase()
    .AddOpenAIClient()
    .AddSemanticKernel(gitHubTools, blueSkyTools)
    .AddAuthentication()
    .AddMassTransitConfig()
    .Configure<GitHubOptions>()
    .Configure<GoogleOptions>()
    .Configure<WeatherOptions>()
    .AddRedisClient("message-cache");

builder.Services.AddSignalR();

builder.Services
    .AddScoped<TokenProvider>()
    .AddHttpContextAccessor()
    .AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

app.MapGet("/authentication/{provider}",
    (string provider, HttpContext context) =>
    {
        var returnUrl = context.Request.Query["returnUrl"].FirstOrDefault() ?? "/";
        return Results.Challenge(
            properties: new AuthenticationProperties { RedirectUri = returnUrl },
            authenticationSchemes: [provider]);
    });

app.MapGet("/logout", async ctx =>
{
    await ctx.SignOutAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new AuthenticationProperties
        {
            RedirectUri = "/"
        });
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// The SignalR hubs used in my talks.
app.MapHub<BotHub>("/bot-hub");

await app.RunAsync();