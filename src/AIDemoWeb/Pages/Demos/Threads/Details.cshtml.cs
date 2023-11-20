using Haack.AIDemoWeb.Entities;
using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

    public Haack.AIDemoWeb.Entities.AssistantThread ThreadEntity { get; private set; } = default!;

    public Haack.AIDemoWeb.Library.Clients.AssistantThread Thread { get; private set; } = default!;

    public IReadOnlyList<Message> Messages { get; private set; } = Array.Empty<Message>();

    public IReadOnlyList<ThreadRun> Runs { get; private set; } = Array.Empty<ThreadRun>();

    [BindProperty]
    public string NewMessageContent { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(string id, CancellationToken cancellationToken = default)
    {
        var threadEntity = await _db.Threads
            .Include(t => t.Creator)
            .SingleOrDefaultAsync(t => t.ThreadId == id, cancellationToken);
        if (threadEntity is null)
        {
            return NotFound();
        }

        var username = User.Identity?.Name;
        var currentUser = await _db.Users.SingleOrDefaultAsync(u => u.Name == username, cancellationToken);

        if (currentUser is null || threadEntity.CreatorId != currentUser.Id)
        {
            return Forbid();
        }

        var thread = await _openAIClient.GetThreadAsync(_options.ApiKey.Require(), id, cancellationToken);

        ThreadEntity = threadEntity;
        Thread = thread;

        var response = await _openAIClient.GetMessagesAsync(_options.ApiKey.Require(), id, cancellationToken);
        Messages = response.Data;

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
}