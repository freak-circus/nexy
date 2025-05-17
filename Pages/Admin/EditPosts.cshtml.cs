using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Nexy.Data;
using Microsoft.AspNetCore.Authorization;

namespace Nexy.Pages.Admin;

[Authorize]
public class EditPostsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditPostsModel(ApplicationDbContext context) => _context = context;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; } // ModelId

    public string ModelName { get; set; } = string.Empty;

    public List<ModelPost> Posts { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var model = await _context.ModelProfiles
            .Include(m => m.Posts)
            .FirstOrDefaultAsync(m => m.Id == Id);

        if (model == null) return NotFound();

        ModelName = model.Name;
        Posts = model.Posts.OrderByDescending(p => p.CreatedAt).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid postId)
    {
        var post = await _context.ModelPosts.FindAsync(postId);
        if (post != null)
        {
            _context.ModelPosts.Remove(post);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage(new { Id });
    }

    public async Task<IActionResult> OnPostUpdateAsync(Guid postId, string caption, string imageUrl)
    {
        var post = await _context.ModelPosts.FindAsync(postId);
        if (post != null)
        {
            post.Caption = caption;
            post.ImageUrl = imageUrl;
            await _context.SaveChangesAsync();
        }

        return RedirectToPage(new { Id });
    }
}