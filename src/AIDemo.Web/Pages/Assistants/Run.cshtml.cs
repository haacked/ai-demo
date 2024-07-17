using System.Security.Claims;
using Haack.AIDemoWeb.Entities;
using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serious;

namespace AIDemoWeb.Demos.Pages.Assistants;

public class RunAssistantPageModel(AIDemoContext db, IOptions<OpenAIOptions> options, IOpenAIClient openAIClient)
    : PageModel
{
    readonly OpenAIOptions _options = options.Value;

    public Assistant Assistant { get; private set; } = default!;

    public string ThreadId { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(string id, CancellationToken cancellationToken = default)
    {
        Assistant = await openAIClient.GetAssistantAsync(_options.ApiKey.Require(), id, cancellationToken);

        // Let's see if the current user has a thread for this assistant.
        var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var currentUser = await db.Users.SingleOrDefaultAsync(u => u.NameIdentifier == nameIdentifier, cancellationToken);

        var threadEntity = await db.Threads
            .Where(t => t.AssistantId == id)
            .SingleOrDefaultAsync(t => t.Creator == currentUser, cancellationToken);

        if (threadEntity is null)
        {
            // Create the thread for this user.
            var createdThread = await openAIClient.CreateThreadAsync(
                _options.ApiKey.Require(),
                Array.Empty<MessageCreateBody>(),
                cancellationToken);

            threadEntity = new Haack.AIDemoWeb.Entities.AssistantThread
            {
                ThreadId = createdThread.Id,
                AssistantId = id,
                Creator = currentUser.Require(),
                CreatorId = currentUser.Id,
            };
            await db.Threads.AddAsync(threadEntity, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        ThreadId = threadEntity.ThreadId;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id, CancellationToken cancellationToken = default)
    {
        // Delete any thread entities associated with this user...
        var username = User.Identity?.Name;
        var currentUser = await db.Users.SingleOrDefaultAsync(u => u.NameIdentifier == username, cancellationToken);
        var threads = await db.Threads
            .Where(t => t.Creator == currentUser && t.AssistantId == id)
            .ToListAsync(cancellationToken);
        db.Threads.RemoveRange(threads);
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToPage();
    }
}