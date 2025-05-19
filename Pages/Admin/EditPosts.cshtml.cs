using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Nexy.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Nexy.Pages.Admin;

[Authorize(Roles = "Admin")]
public class EditPostsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EditPostsModel> _logger;

    public EditPostsModel(ApplicationDbContext context, ILogger<EditPostsModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; } // ModelId

    public string ModelName { get; set; } = string.Empty;

    public List<ModelPost> Posts { get; set; } = new();

    [BindProperty]
    public List<PostInput> PostsInput { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var model = await _context.ModelProfiles
            .Include(m => m.Posts)
            .FirstOrDefaultAsync(m => m.Id == Id);

        if (model == null)
        {
            _logger.LogWarning("Model not found for Id: {Id}", Id);
            return NotFound();
        }

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
        else
        {
            _logger.LogWarning("Post not found for deletion: {PostId}", postId);
        }

        return RedirectToPage(new { Id });
    }

    public async Task<IActionResult> OnPostUpdateAllAsync()
    {
        try
        {
            foreach (var input in PostsInput)
            {
                var post = await _context.ModelPosts.FindAsync(input.Id);
                if (post != null)
                {
                    post.Caption = input.Caption ?? string.Empty;
                    post.ImageUrl = input.ImageUrl ?? string.Empty;
                    post.IsNsfw = input.IsNsfw;
                }
                else
                {
                    _logger.LogWarning("Post not found for update: {PostId}", input.Id);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToPage(new { Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating posts");
            ModelState.AddModelError("", "Ошибка при обновлении постов. Попробуйте снова.");
            return await OnGetAsync();
        }
    }

    public class PostInput
    {
        public Guid Id { get; set; }
        public string Caption { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsNsfw { get; set; }
    }
}