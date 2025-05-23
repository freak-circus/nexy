using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nexy.Data;
using Microsoft.AspNetCore.Authorization;
using System;

namespace Nexy.Pages.Admin;

[Authorize(Roles = "Admin")]
public class AddPostsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public AddPostsModel(ApplicationDbContext context) => _context = context;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; } // ModelId

    [BindProperty]
    public string RawInput { get; set; } = string.Empty;

    public string ModelName { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        var model = await _context.ModelProfiles.FindAsync(Id);
        if (model == null) return NotFound();

        ModelName = model.Name;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var model = await _context.ModelProfiles.FindAsync(Id);
        if (model == null) return NotFound();

        var lines = RawInput.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        var posts = new List<ModelPost>();
        var random = new Random();
        var startDate = new DateTime(2024, 8, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = DateTime.UtcNow;
        var timeSpan = endDate - startDate;

        for (int i = 0; i < lines.Length - 1; i += 2)
        {
            string caption = lines[i].Trim();
            string imageUrl = lines[i + 1].Trim();
            bool isNsfw = false;

            if (imageUrl.EndsWith("!"))
            {
                isNsfw = true;
                imageUrl = imageUrl.TrimEnd('!');
            }

            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                // Генерация случайной даты
                var randomSeconds = random.NextDouble() * timeSpan.TotalSeconds;
                var randomDate = startDate.AddSeconds(randomSeconds);

                posts.Add(new ModelPost
                {
                    Id = Guid.NewGuid(),
                    ModelId = model.Id,
                    Caption = caption,
                    ImageUrl = imageUrl,
                    CreatedAt = randomDate,
                    IsNsfw = isNsfw
                });
            }
        }

        if (posts.Count > 0)
        {
            _context.ModelPosts.AddRange(posts);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("ModelList");
    }
}