using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nexy.Data;
using Microsoft.AspNetCore.Authorization;

namespace Nexy.Pages.Admin;

[Authorize(Roles = "Admin")]
public class EditModelModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModelModel(ApplicationDbContext context) => _context = context;

    [BindProperty]
    public ModelProfile Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var model = await _context.ModelProfiles.FindAsync(id);
        if (model == null) return NotFound();
        Input = model;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var model = await _context.ModelProfiles.FindAsync(Input.Id);
        if (model == null) return NotFound();

        model.Name = Input.Name;
        model.Description = Input.Description;
        model.AvatarUrl = Input.AvatarUrl;
        model.IsActive = Input.IsActive;

        await _context.SaveChangesAsync();
        return RedirectToPage("ModelList");
    }
}