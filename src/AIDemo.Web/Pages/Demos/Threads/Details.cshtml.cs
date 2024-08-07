using Haack.AIDemoWeb.Entities;
using Haack.AIDemoWeb.Library;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OpenAI;
using OpenAI.Assistants;
using Serious;
using AssistantThread = OpenAI.Assistants.AssistantThread;

namespace AIDemoWeb.Demos.Pages.Assistants;

public class ThreadDetailsPageModel(AIDemoDbContext db, OpenAIClient openAIClient) : PageModel
{
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

    public AssistantThread Thread { get; private set; } = default!;

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

        var username = User.Identity?.Name;
        var currentUser = await db.Users.SingleOrDefaultAsync(u => u.NameIdentifier == username, cancellationToken);

        if (currentUser is null || threadEntity.CreatorId != currentUser.Id)
        {
            return Forbid();
        }

#pragma warning disable OPENAI001
        var assistantClient = openAIClient.GetAssistantClient();
#pragma warning restore OPENAI001
        var thread = await assistantClient.GetThreadAsync(id, cancellationToken);

        ThreadEntity = threadEntity;
        Thread = thread;

        Messages = await assistantClient.GetMessagesAsync(
            id,
            options: new MessageCollectionOptions { Order = ListOrder.OldestFirst },
            cancellationToken: cancellationToken)
            .GetAllValuesAsync(cancellationToken: cancellationToken)
            .ToReadOnlyListAsync(cancellationToken);

        var assistantsResponse = await assistantClient
            .GetAssistantsAsync(cancellationToken: cancellationToken)
            .GetAllValuesAsync(cancellationToken)
            .ToReadOnlyListAsync(cancellationToken);
        Assistants = assistantsResponse.Select(a => new SelectListItem(a.Name, a.Id)).ToList();

        Runs = await assistantClient.GetRunsAsync(
            id,
            new RunCollectionOptions { Order = ListOrder.OldestFirst },
            cancellationToken: cancellationToken)
            .GetAllValuesAsync(cancellationToken)
            .ToReadOnlyListAsync(cancellationToken);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

#pragma warning disable OPENAI001
        var assistantClient = openAIClient.GetAssistantClient();
#pragma warning restore OPENAI001

        var message = await assistantClient.CreateMessageAsync(
            id,
            MessageRole.User,
            new[] { MessageContent.FromText(NewMessageContent) },
            cancellationToken: cancellationToken);

        StatusMessage = $"Message {message.Value.Id} added to the thread.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCreateRunAsync(string id, CancellationToken cancellationToken = default)
    {
#pragma warning disable OPENAI001
        var assistantClient = openAIClient.GetAssistantClient();
#pragma warning restore OPENAI001

        var run = await assistantClient.CreateRunAsync(
            id,
            AssistantIdForRun.Require(),
            cancellationToken: cancellationToken);

        StatusMessage = $"Run {run.Value.Id} created.";

        return RedirectToPage();
    }
}