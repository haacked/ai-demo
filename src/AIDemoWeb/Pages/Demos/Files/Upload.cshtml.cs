using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Refit;
using Serious;

namespace AIDemoWeb.Demos.Pages.Files;

public class UploadPageModel : PageModel
{
    readonly OpenAIOptions _options;
    readonly IOpenAIClient _openAIClient;

    [BindProperty]
    public IFormFile Upload { get; set; } = null!;

    [BindProperty]
    public string Purpose { get; set; } = null!;

    [TempData]
    public string? StatusMessage { get; set; }

    public UploadPageModel(IOptions<OpenAIOptions> options, IOpenAIClient openAIClient)
    {
        _options = options.Value;
        _openAIClient = openAIClient;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var streamPart = new StreamPart(Upload.OpenReadStream(), Upload.FileName);
        var result = await _openAIClient.UploadFileAsync(_options.ApiKey.Require(), Purpose, streamPart);

        StatusMessage = $"File {result.Id} uploaded.";

        return RedirectToPage("Index");
    }
}