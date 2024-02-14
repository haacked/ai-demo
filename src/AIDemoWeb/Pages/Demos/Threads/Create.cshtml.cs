using Haack.AIDemoWeb.Entities;
using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Refit;
using Serious;
using AssistantThread = Haack.AIDemoWeb.Entities.AssistantThread;

#pragma warning disable CA2227, CA1002, CA1819

namespace AIDemoWeb.Demos.Pages.Assistants;

public class CreateThreadPageModel(AIDemoContext db, IOptions<OpenAIOptions> options, IOpenAIClient openAIClient)
    : PageModel
{
    readonly OpenAIOptions _options = options.Value;

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var createdThread = await openAIClient.CreateThreadAsync(
                _options.ApiKey.Require(),
                Array.Empty<MessageCreateBody>(),
                cancellationToken);

            StatusMessage = $"Assistant {createdThread.Id} created.";

            var username = User.Identity?.Name;
            var currentUser = await db.Users.SingleOrDefaultAsync(u => u.Name == username, cancellationToken);

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
            await db.Threads.AddAsync(threadEntity, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return RedirectToPage("Index");
        }
        catch (ApiException e)
        {
            ModelState.AddModelError("", e.Content ?? e.Message);
            return Page();
        }
    }
}