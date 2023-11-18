using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Serious;
using File = Haack.AIDemoWeb.Library.Clients.File;

namespace AIDemoWeb.Demos.Pages.Files;

public class FilesIndexPageModel : PageModel
{
    readonly OpenAIOptions _options;
    readonly IOpenAIClient _openAIClient;

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public string? FileIdToDelete { get; set; }

    public IReadOnlyList<File> Files { get; private set; } = Array.Empty<File>();

    public FilesIndexPageModel(IOptions<OpenAIOptions> options, IOpenAIClient openAIClient)
    {
        _options = options.Value;
        _openAIClient = openAIClient;
    }

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        var response = await _openAIClient.GetFilesAsync(_options.ApiKey.Require(), cancellationToken: cancellationToken);
        Files = response.Data;
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
    {
        var response = await _openAIClient.DeleteFileAsync(
            _options.ApiKey.Require(),
            FileIdToDelete.Require(),
            cancellationToken);
        StatusMessage = $"File {response.Id} deleted.";
        return RedirectToPage();
    }
}