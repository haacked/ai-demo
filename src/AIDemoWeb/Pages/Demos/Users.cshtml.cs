using Haack.AIDemoWeb.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AIDemoWeb.Demos.Pages;

[AllowAnonymous]
public class UsersPageModel : PageModel
{
    readonly AIDemoContext _db;

    public UsersPageModel(AIDemoContext db)
    {
        _db = db;
    }


    public IReadOnlyList<User> Users { get; private set; } = null!;

    public async Task OnGetAsync()
    {
        Users = await _db.Users.ToListAsync();
    }
}