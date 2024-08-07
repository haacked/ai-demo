using System.ClientModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenAI;

namespace AIDemoWeb.Demos.Pages.Files;

public class UploadPageModel(OpenAIClient openAIClient)
    : PageModel
{
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

        var fileClient = openAIClient.GetFileClient();

        using var binaryContent = BinaryContent.Create(Upload.OpenReadStream());

        // Use the binaryContent object as needed
        var result = await fileClient.UploadFileAsync(
            binaryContent,
            Upload.ContentType,
            options: null);
        StatusMessage = $"File \"{Upload.FileName}\" uploaded.";

        return RedirectToPage("Index");
    }
}