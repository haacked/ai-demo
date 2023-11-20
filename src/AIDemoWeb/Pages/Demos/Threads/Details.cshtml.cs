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

public class ThreadDetailsPageModel : PageModel
{
    readonly AIDemoContext _db;
    readonly OpenAIOptions _options;
    readonly IOpenAIClient _openAIClient;

    [TempData]
    public string? StatusMessage { get; set; }

    public ThreadDetailsPageModel(AIDemoContext db, IOptions<OpenAIOptions> options, IOpenAIClient openAIClient)
    {
        _db = db;
        _options = options.Value;
        _openAIClient = openAIClient;
    }

    [BindProperty]
    public string NewMessageContent { get; set; } = default!;

    [BindProperty]
    public string? AssistantIdForRun { get; set; }

    [BindProperty]
    public string? RunIdToDelete { get; set; }

    public IReadOnlyList<SelectListItem> Assistants { get; private set; } = Array.Empty<SelectListItem>();

    public Haack.AIDemoWeb.Entities.AssistantThread ThreadEntity { get; private set; } = default!;

    public Haack.AIDemoWeb.Library.Clients.AssistantThread Thread { get; private set; } = default!;

    public IReadOnlyList<Message> Messages { get; private set; } = Array.Empty<Message>();

    public IReadOnlyList<ThreadRun> Runs { get; private set; } = Array.Empty<ThreadRun>();

    public async Task<IActionResult> OnGetAsync(string id, CancellationToken cancellationToken = default)
    {
        var threadEntity = await _db.Threads
            .Include(t => t.Creator)
            .SingleOrDefaultAsync(t => t.ThreadId == id, cancellationToken);
        if (threadEntity is null)
        {
            return NotFound();
        }

        var apiKey = _options.ApiKey.Require();
        var username = User.Identity?.Name;
        var currentUser = await _db.Users.SingleOrDefaultAsync(u => u.Name == username, cancellationToken);

        if (currentUser is null || threadEntity.CreatorId != currentUser.Id)
        {
            return Forbid();
        }

        var thread = await _openAIClient.GetThreadAsync(apiKey, id, cancellationToken);

        ThreadEntity = threadEntity;
        Thread = thread;

        var response = await _openAIClient.GetMessagesAsync(apiKey, id, order: "asc", cancellationToken: cancellationToken);
        Messages = response.Data;

        var assistantsResponse = await _openAIClient.GetAssistantsAsync(apiKey, cancellationToken);
        Assistants = assistantsResponse.Data.Select(a => new SelectListItem(a.Name, a.Id)).ToList();

        var runsResponse = await _openAIClient.GetRunsAsync(apiKey, id, order: "asc", cancellationToken: cancellationToken);
        Runs = runsResponse.Data;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var message = await _openAIClient.CreateMessageAsync(
            _options.ApiKey.Require(),
            id,
            new MessageCreateBody { Content = NewMessageContent },
            cancellationToken);

        StatusMessage = $"Message {message.Id} added to the thread.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCreateRunAsync(string id, CancellationToken cancellationToken = default)
    {
        var run = await _openAIClient.CreateRunAsync(
            _options.ApiKey.Require(),
            id,
            new ThreadRunCreateBody { AssistantId = AssistantIdForRun.Require() },
            cancellationToken);

        StatusMessage = $"Run {run.Id} created.";

        return RedirectToPage();
    }
}