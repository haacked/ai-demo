using Haack.AIDemoWeb.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AIDemoWeb.Demos.Pages;

[AllowAnonymous]
public class UsersPageModel : PageModel
{
    readonly AIDemoContext _db;

    [BindProperty]
    public int? FactId { get; set; }

    public UsersPageModel(AIDemoContext db)
    {
        _db = db;
    }

    public IReadOnlyList<User> Users { get; private set; } = null!;

    public async Task OnGetAsync()
    {
        Users = await _db.Users.Include(u => u.Facts).ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (FactId.HasValue)
        {
            var fact = await _db.UserFacts.FindAsync(FactId.Value);
            if (fact is not null)
            {
                _db.UserFacts.Remove(fact);
                await _db.SaveChangesAsync();
            }
        }

        return RedirectToPage();
    }
}