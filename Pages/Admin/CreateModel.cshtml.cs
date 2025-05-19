using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nexy.Data;
using Microsoft.AspNetCore.Authorization;

namespace Nexy.Pages.Admin;

[Authorize(Roles = "Admin")]
public class CreateModelModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModelModel(ApplicationDbContext context) => _context = context;

    [BindProperty]
    public ModelProfile Input { get; set; } = new();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        _context.ModelProfiles.Add(Input);
        await _context.SaveChangesAsync();
        return RedirectToPage("ModelList");
    }
}