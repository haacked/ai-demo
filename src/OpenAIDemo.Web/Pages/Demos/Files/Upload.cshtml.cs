using Haack.AIDemoWeb.Library.Clients;
using Haack.AIDemoWeb.Startup.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Refit;
using Serious;

namespace AIDemoWeb.Demos.Pages.Files;

public class UploadPageModel(IOptions<OpenAIOptions> options, IOpenAIClient openAIClient)
    : PageModel
{
    readonly OpenAIOptions _options = options.Value;

    [BindProperty]
    public IFormFile Upload { get; set; } = null!;

    [BindProperty]
    public string Purpose { get; set; } = null!;

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var streamPart = new StreamPart(Upload.OpenReadStream(), Upload.FileName);
        var result = await openAIClient.UploadFileAsync(_options.ApiKey.Require(), Purpose, streamPart);

        StatusMessage = $"File {result.Id} uploaded.";

        return RedirectToPage("Index");
    }
}