using Haack.AIDemoWeb.Startup;
using Haack.AIDemoWeb.Startup.Config;
using Haack.AIDemoWeb.Components; // Rider highlights this line for some reason, but it's legit. It compiles.
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

// Registers my OpenAI client accessor and configures it.
builder.Services.RegisterOpenAI(builder.Configuration);
builder.Services.Configure<GitHubOptions>(builder.Configuration.GetSection(GitHubOptions.GitHub));
builder.Services.Configure<GoogleOptions>(builder.Configuration.GetSection(GoogleOptions.Google));
builder.Services.Configure<WeatherOptions>(builder.Configuration.GetSection(WeatherOptions.Weather));
builder.Services.AddAuthentication(builder.Configuration);
builder.Services.AddMigrationServices();
builder.Services.AddSignalR();
builder.Services.AddMassTransitConfig();

// Register all the GPT functions in the specified assembly.
// I wrote this to make calling GPT functions easier.
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

// A legacy SignalR Hub I don't use in my talk, but keep around for reference.
app.MapHub<MultiUserChatHub>("/chat-hub");


app.Run();
