using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Serious;
using File = Haack.AIDemoWeb.Library.Clients.File;

namespace AIDemoWeb.Demos.Pages.Files;

public class FilesIndexPageModel(IOptions<OpenAIOptions> options, IOpenAIClient openAIClient)
    : PageModel
{
    readonly OpenAIOptions _options = options.Value;

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public string? FileIdToDelete { get; set; }

    public IReadOnlyList<File> Files { get; private set; } = Array.Empty<File>();

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        var response = await openAIClient.GetFilesAsync(_options.ApiKey.Require(), cancellationToken: cancellationToken);
        Files = response.Data;
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
    {
        var response = await openAIClient.DeleteFileAsync(
            _options.ApiKey.Require(),
            FileIdToDelete.Require(),
            cancellationToken);
        StatusMessage = $"File {response.Id} deleted.";
        return RedirectToPage();
    }
}