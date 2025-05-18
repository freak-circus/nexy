using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Nexy.Data;

namespace Nexy.Pages;

public class FeedModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private const int PageSize = 10;

    public FeedModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<ModelPost> Posts { get; set; } = new();
    public int LoadedCount { get; set; }

    public async Task OnGetAsync()
    {
        var allIds = await _context.ModelPosts
            .Include(p => p.Model)
            .Where(p => p.Model.IsActive)
            .Select(p => p.Id)
            .ToListAsync();

        // Перемешиваем
        var rnd = new Random();
        var shuffledIds = allIds.OrderBy(_ => rnd.Next()).ToList();

        TempData["PostOrder"] = string.Join(",", shuffledIds);

        var firstIds = shuffledIds.Take(PageSize).ToList();

        Posts = await _context.ModelPosts
            .Include(p => p.Model)
            .Where(p => firstIds.Contains(p.Id))
            .ToListAsync();

        // Упорядочиваем так же, как в списке
        Posts = firstIds.Select(id => Posts.First(p => p.Id == id)).ToList();

        LoadedCount = Posts.Count;
    }

    public async Task<PartialViewResult> OnGetLoadMoreAsync(int skip)
    {
        if (TempData["PostOrder"] is not string raw) return Partial("_FeedPostListPartial", new List<ModelPost>());

        TempData.Keep("PostOrder");
        var ids = raw.Split(',').Select(Guid.Parse).ToList();

        var nextIds = ids.Skip(skip).Take(PageSize).ToList();
        var posts = await _context.ModelPosts
            .Include(p => p.Model)
            .Where(p => nextIds.Contains(p.Id))
            .ToListAsync();

        posts = nextIds.Select(id => posts.First(p => p.Id == id)).ToList();

        return Partial("_FeedPostListPartial", posts);
    }
}