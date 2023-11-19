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

public class ThreadsIndexPageModel : PageModel
{
    readonly OpenAIOptions _options;
    readonly AIDemoContext _db;
    readonly IOpenAIClient _openAIClient;

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public string? ThreadIdToDelete { get; set; }

    public IReadOnlyList<AssistantThread> Threads { get; private set; } = Array.Empty<AssistantThread>();

    public ThreadsIndexPageModel(AIDemoContext db, IOptions<OpenAIOptions> options, IOpenAIClient openAIClient)
    {
        _db = db;
        _options = options.Value;
        _openAIClient = openAIClient;
    }

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        Threads = await _db.Threads.Include(t => t.Creator).ToListAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
    {
        var threadToDelete = await _db.Threads.SingleOrDefaultAsync(
            t => t.ThreadId == ThreadIdToDelete, cancellationToken);

        if (threadToDelete is null)
        {
            StatusMessage = $"Thread {ThreadIdToDelete} not found.";
            return RedirectToPage();
        }

        _db.Threads.Remove(threadToDelete);
        await _db.SaveChangesAsync(cancellationToken);

        var response = await _openAIClient.DeleteAssistantAsync(
            _options.ApiKey.Require(),
            ThreadIdToDelete.Require(),
            cancellationToken);
        StatusMessage = $"Assistant {response.Id} deleted.";
        return RedirectToPage();
    }
}