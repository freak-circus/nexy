using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Nexy.Data;

namespace Nexy.Pages.Model
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public ModelProfile? Profile { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Profile = await _context.ModelProfiles
                .Include(m => m.Posts)
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);

            if (Profile == null)
                return NotFound();

            return Page();
        }
    }
}