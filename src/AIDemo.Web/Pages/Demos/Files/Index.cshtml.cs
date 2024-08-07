using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenAI;
using OpenAI.Files;
using Serious;

namespace AIDemoWeb.Demos.Pages.Files;

public class FilesIndexPageModel(OpenAIClient openAIClient) : PageModel
{
    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public string? FileIdToDelete { get; set; }

    public IReadOnlyList<OpenAIFileInfo> Files { get; private set; } = Array.Empty<OpenAIFileInfo>();

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        var fileClient = openAIClient.GetFileClient();
        var response = await fileClient.GetFilesAsync(cancellationToken: cancellationToken);
        Files = response.Value;
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
    {
        var fileClient = openAIClient.GetFileClient();

        await fileClient.DeleteFileAsync(
            FileIdToDelete.Require(),
            cancellationToken);

        StatusMessage = $"File {FileIdToDelete} deleted.";
        return RedirectToPage();
    }
}