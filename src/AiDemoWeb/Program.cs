using Haack.AIDemoWeb.Startup;
using Haack.AIDemoWeb.Startup.Config;
using Haack.AIDemoWeb.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using OpenAIDemo.Hubs;
using Serious;
using Serious.ChatFunctions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddRazorPages()
    .AddRazorRuntimeCompilation();
builder.Services.AddClients();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.RegisterOpenAI(builder.Configuration);
builder.Services.Configure<GitHubOptions>(builder.Configuration.GetSection(GitHubOptions.GitHub));
builder.Services.AddAuthentication(builder.Configuration);
builder.Services.AddMigrationServices();
builder.Services.AddSignalR();
builder.Services.AddMassTransitConfig();
builder.Services.Configure<WeatherOptions>(builder.Configuration.GetSection(WeatherOptions.Weather));
builder.Services.AddFunctionDispatcher(typeof(WeatherOptions).Assembly.Require());

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
app.MapRazorComponents<App>()
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

app.MapHub<MultiUserChatHub>("/chat-hub");
app.MapHub<AssistantHub>("/assistant-hub");
app.MapHub<BotHub>("/bot-hub");

app.Run();
