using AIDemo.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpenAI;
using Serious;
using AssistantThread = AIDemo.Entities.AssistantThread;

namespace AIDemoWeb.Demos.Pages.Assistants;

public class ThreadsIndexPageModel(AIDemoDbContext db, OpenAIClient openAIClient)
    : PageModel
{
    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public string? ThreadIdToDelete { get; set; }

    public IReadOnlyList<AssistantThread> Threads { get; private set; } = Array.Empty<AssistantThread>();

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        Threads = await db.Threads.Include(t => t.Creator).ToListAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
    {
        var threadToDelete = await db.Threads.SingleOrDefaultAsync(
            t => t.ThreadId == ThreadIdToDelete, cancellationToken);

        if (threadToDelete is null)
        {
            StatusMessage = $"Thread {ThreadIdToDelete} not found.";
            return RedirectToPage();
        }

        db.Threads.Remove(threadToDelete);
        await db.SaveChangesAsync(cancellationToken);

#pragma warning disable OPENAI001
        var assistantClient = openAIClient.GetAssistantClient();
#pragma warning restore OPENAI001

        await assistantClient.DeleteThreadAsync(
            ThreadIdToDelete.Require(),
            cancellationToken);
        StatusMessage = $"Thread {ThreadIdToDelete} deleted.";
        return RedirectToPage();
    }
}