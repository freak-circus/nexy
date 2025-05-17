using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Nexy.Data;
using Microsoft.AspNetCore.Authorization;

namespace Nexy.Pages.Admin;

[Authorize]
public class ModelListModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public ModelListModel(ApplicationDbContext context) => _context = context;

    public List<ModelProfile> Models { get; set; } = new();

    public async Task OnGetAsync()
    {
        Models = await _context.ModelProfiles.OrderByDescending(x => x.IsActive).ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var model = await _context.ModelProfiles.FindAsync(id);
        if (model != null)
        {
            _context.ModelProfiles.Remove(model);
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}