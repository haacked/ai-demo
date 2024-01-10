using Haack.AIDemoWeb.Entities;
using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serious;
using AssistantThread = Haack.AIDemoWeb.Entities.AssistantThread;

namespace AIDemoWeb.Pages;

[AllowAnonymous]
public class HomePageModel : PageModel
{
    readonly OpenAIOptions _options;
    readonly AIDemoContext _db;
    readonly IOpenAIClient _openAIClient;
    readonly GitHubOptions _gitHubOptions;

    public string? AssistantName => Assistant?.Name;

    public Assistant? Assistant { get; private set; }

    public string? ThreadId { get; private set; }

    public HomePageModel(
        AIDemoContext db,
        IOptions<OpenAIOptions> options,
        IOpenAIClient openAIClient,
        IOptions<GitHubOptions> githubOptions)
    {
        _options = options.Value;
        _db = db;
        _openAIClient = openAIClient;
        _gitHubOptions = githubOptions.Value;
    }

    public async Task<IActionResult> OnGetAsync(string? id, CancellationToken cancellationToken = default)
    {
        if (_gitHubOptions.Host is not null && Request.Host.Host != _gitHubOptions.Host)
        {
            return Redirect($"https://{_gitHubOptions.Host}/");
        }


        // Let's see if the current user has a thread for this assistant.
        var username = User.Identity?.Name;
        var currentUser = await _db.Users.SingleOrDefaultAsync(u => u.Name == username, cancellationToken);

        if (id is not null)
        {
            Assistant = await _openAIClient.GetAssistantAsync(_options.ApiKey.Require(), id, cancellationToken);

            var threadEntity = await _db.Threads
                .Where(t => t.AssistantId == id)
                .SingleOrDefaultAsync(t => t.Creator == currentUser, cancellationToken);

            if (threadEntity is null)
            {
                // Create the thread for this user.
                var createdThread = await _openAIClient.CreateThreadAsync(
                    _options.ApiKey.Require(),
                    Array.Empty<MessageCreateBody>(),
                    cancellationToken);

                threadEntity = new AssistantThread
                {
                    ThreadId = createdThread.Id,
                    AssistantId = id,
                    Creator = currentUser.Require(),
                    CreatorId = currentUser.Id,
                };
                await _db.Threads.AddAsync(threadEntity, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
            }

            ThreadId = threadEntity.ThreadId;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id, CancellationToken cancellationToken = default)
    {
        // Delete any thread entities associated with this user...
        var username = User.Identity?.Name;
        var currentUser = await _db.Users.SingleOrDefaultAsync(u => u.Name == username, cancellationToken);
        var threads = await _db.Threads
            .Where(t => t.Creator == currentUser && t.AssistantId == id)
            .ToListAsync(cancellationToken);
        _db.Threads.RemoveRange(threads);
        await _db.SaveChangesAsync(cancellationToken);
        return RedirectToPage();
    }
}