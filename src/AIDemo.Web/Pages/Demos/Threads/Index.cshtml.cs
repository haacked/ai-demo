using Haack.AIDemoWeb.Entities;
using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serious;
using AssistantThread = Haack.AIDemoWeb.Entities.AssistantThread;

namespace AIDemoWeb.Demos.Pages.Assistants;

public class ThreadsIndexPageModel(AIDemoDbContext db, IOptions<OpenAIOptions> options, IOpenAIClient openAIClient)
    : PageModel
{
    readonly OpenAIOptions _options = options.Value;

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

        var response = await openAIClient.DeleteThreadAsync(
            _options.ApiKey.Require(),
            ThreadIdToDelete.Require(),
            cancellationToken);
        StatusMessage = $"Thread {response.Id} deleted.";
        return RedirectToPage();
    }
}