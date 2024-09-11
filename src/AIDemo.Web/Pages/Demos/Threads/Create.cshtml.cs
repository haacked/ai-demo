using AIDemo.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpenAI;
using Refit;
using AssistantThread = AIDemo.Entities.AssistantThread;

#pragma warning disable CA2227, CA1002, CA1819

namespace AIDemoWeb.Demos.Pages.Assistants;

public class CreateThreadPageModel(AIDemoDbContext db, OpenAIClient openAIClient)
    : PageModel
{
    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

#pragma warning disable OPENAI001
        var assistantClient = openAIClient.GetAssistantClient();
#pragma warning restore OPENAI001
        try
        {
            var createdThread = await assistantClient.CreateThreadAsync(
                cancellationToken: cancellationToken);

            StatusMessage = $"Assistant {createdThread.Value.Id} created.";

            var username = User.Identity?.Name;
            var currentUser = await db.Users.SingleOrDefaultAsync(u => u.NameIdentifier == username, cancellationToken);

            if (currentUser is null)
            {
                StatusMessage = "You must be logged in to create a thread.";
                return RedirectToPage();
            }

            var threadEntity = new AssistantThread
            {
                ThreadId = createdThread.Value.Id,
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