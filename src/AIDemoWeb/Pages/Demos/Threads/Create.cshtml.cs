using Haack.AIDemoWeb.Entities;
using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Refit;
using Serious;
using AssistantThread = Haack.AIDemoWeb.Entities.AssistantThread;

#pragma warning disable CA2227, CA1002, CA1819

namespace AIDemoWeb.Demos.Pages.Assistants;

public class CreateThreadPageModel : PageModel
{
    readonly OpenAIOptions _options;
    readonly AIDemoContext _db;
    readonly IOpenAIClient _openAIClient;

    [TempData]
    public string? StatusMessage { get; set; }

    public IReadOnlyList<SelectListItem> AvailableFileIds { get; set; } = Array.Empty<SelectListItem>();

    public IReadOnlyList<string> AvailableTools { get; set; } = new [] { "code_interpreter", "retrieval" };

    public CreateThreadPageModel(AIDemoContext db, IOptions<OpenAIOptions> options, IOpenAIClient openAIClient)
    {
        _db = db;
        _options = options.Value;
        _openAIClient = openAIClient;
    }

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        var response = await _openAIClient.GetFilesAsync(_options.ApiKey.Require(), "assistants", cancellationToken);
        AvailableFileIds = response.Data.Select(f => new SelectListItem(f.Filename, f.Id)).ToList();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var createdThread = await _openAIClient.CreateThreadAsync(
                _options.ApiKey.Require(),
                Array.Empty<Message>(),
                cancellationToken);

            StatusMessage = $"Assistant {createdThread.Id} created.";

            var username = User.Identity?.Name;
            var currentUser = await _db.Users.SingleOrDefaultAsync(u => u.Name == username, cancellationToken);

            if (currentUser is null)
            {
                StatusMessage = "You must be logged in to create a thread.";
                return RedirectToPage();
            }

            var threadEntity = new AssistantThread
            {
                ThreadId = createdThread.Id,
                Creator = currentUser,
                CreatorId = currentUser.Id,
            };
            await _db.Threads.AddAsync(threadEntity, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return RedirectToPage("Index");
        }
        catch (ApiException e)
        {
            ModelState.AddModelError("", e.Content ?? e.Message);
            return Page();
        }
    }
}