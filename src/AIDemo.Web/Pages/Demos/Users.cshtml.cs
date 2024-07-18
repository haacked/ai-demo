using Haack.AIDemoWeb.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AIDemoWeb.Demos.Pages;

[AllowAnonymous]
public class UsersPageModel(AIDemoDbContext db) : PageModel
{
    [BindProperty]
    public int? FactId { get; set; }

    [BindProperty]
    public int? UserId { get; set; }

    public IReadOnlyList<User> Users { get; private set; } = null!;

    public async Task OnGetAsync()
    {
        Users = await db.Users.Include(u => u.Facts).ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (FactId.HasValue)
        {
            var fact = await db.UserFacts.FindAsync(FactId.Value);
            if (fact is not null)
            {
                db.UserFacts.Remove(fact);
                await db.SaveChangesAsync();
            }
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteUserAsync()
    {
        if (UserId.HasValue)
        {
            var user = await db.Users.FindAsync(UserId.Value);
            if (user is not null)
            {
                db.Users.Remove(user);
                await db.SaveChangesAsync();
            }
        }

        return RedirectToPage();
    }
}