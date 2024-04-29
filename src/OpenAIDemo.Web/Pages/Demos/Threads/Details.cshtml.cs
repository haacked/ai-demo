using Haack.AIDemoWeb.Entities;
using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serious;

namespace AIDemoWeb.Demos.Pages.Assistants;

public class ThreadDetailsPageModel(AIDemoContext db, IOptions<OpenAIOptions> options, IOpenAIClient openAIClient)
    : PageModel
{
    readonly OpenAIOptions _options = options.Value;

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public string NewMessageContent { get; set; } = default!;

    [BindProperty]
    public string? AssistantIdForRun { get; set; }

    [BindProperty]
    public string? RunIdToDelete { get; set; }

    public IReadOnlyList<SelectListItem> Assistants { get; private set; } = Array.Empty<SelectListItem>();

    public Haack.AIDemoWeb.Entities.AssistantThread ThreadEntity { get; private set; } = default!;

    public Haack.AIDemoWeb.Library.Clients.AssistantThread Thread { get; private set; } = default!;

    public IReadOnlyList<ThreadMessage> Messages { get; private set; } = Array.Empty<ThreadMessage>();

    public IReadOnlyList<ThreadRun> Runs { get; private set; } = Array.Empty<ThreadRun>();

    public async Task<IActionResult> OnGetAsync(string id, CancellationToken cancellationToken = default)
    {
        var threadEntity = await db.Threads
            .Include(t => t.Creator)
            .SingleOrDefaultAsync(t => t.ThreadId == id, cancellationToken);
        if (threadEntity is null)
        {
            return NotFound();
        }

        var apiKey = _options.ApiKey.Require();
        var username = User.Identity?.Name;
        var currentUser = await db.Users.SingleOrDefaultAsync(u => u.NameIdentifier == username, cancellationToken);

        if (currentUser is null || threadEntity.CreatorId != currentUser.Id)
        {
            return Forbid();
        }

        var thread = await openAIClient.GetThreadAsync(apiKey, id, cancellationToken);

        ThreadEntity = threadEntity;
        Thread = thread;

        var response = await openAIClient.GetMessagesAsync(apiKey, id, order: "asc", cancellationToken: cancellationToken);
        Messages = response.Data;

        var assistantsResponse = await openAIClient.GetAssistantsAsync(apiKey, cancellationToken);
        Assistants = assistantsResponse.Data.Select(a => new SelectListItem(a.Name, a.Id)).ToList();

        var runsResponse = await openAIClient.GetRunsAsync(apiKey, id, order: "asc", cancellationToken: cancellationToken);
        Runs = runsResponse.Data;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var message = await openAIClient.CreateMessageAsync(
            _options.ApiKey.Require(),
            id,
            new MessageCreateBody { Content = NewMessageContent },
            cancellationToken);

        StatusMessage = $"Message {message.Id} added to the thread.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCreateRunAsync(string id, CancellationToken cancellationToken = default)
    {
        var run = await openAIClient.CreateRunAsync(
            _options.ApiKey.Require(),
            id,
            new ThreadRunCreateBody { AssistantId = AssistantIdForRun.Require() },
            cancellationToken);

        StatusMessage = $"Run {run.Id} created.";

        return RedirectToPage();
    }
}