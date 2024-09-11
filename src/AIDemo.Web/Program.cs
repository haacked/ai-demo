using Haack.AIDemoWeb.Startup;
using Haack.AIDemoWeb.Startup.Config;
using Haack.AIDemoWeb.Components; // Rider highlights this line for some reason, but it's legit. It compiles.
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using AIDemo.Hubs;
using AIDemo.Library;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder
    .AddServiceDefaults()
    .AddClients()
    .AddDatabase()
    .AddOpenAIClient()
    .AddSemanticKernel()
    .AddAuthentication()
    .AddMassTransitConfig()
    .Configure<GitHubOptions>()
    .Configure<GoogleOptions>()
    .Configure<WeatherOptions>()
    .AddRedisClient("message-cache");

builder.Services.AddSignalR();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddRazorPages()
    .AddRazorRuntimeCompilation();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorPages().RequireAuthorization();
app.MapRazorComponents<App>() // Rider highlights this line for some reason, but it's legit. It compiles.
    .AddInteractiveServerRenderMode();

app.MapGet("/logout", async ctx =>
{
    await ctx.SignOutAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new AuthenticationProperties
        {
            RedirectUri = "/"
        });
});

// The SignalR hubs used in my talks.
app.MapHub<AssistantHub>("/assistant-hub");
app.MapHub<BotHub>("/bot-hub");

app.Run();
