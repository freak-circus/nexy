using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Nexy.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            var query = HttpContext.Request.Query;
            var utmData = new
            {
                Timestamp = DateTime.UtcNow.ToString("o"),
                UtmSource = query.ContainsKey("utm_source") ? query["utm_source"].ToString() : string.Empty,
                UtmMedium = query.ContainsKey("utm_medium") ? query["utm_medium"].ToString() : string.Empty,
                UtmCampaign = query.ContainsKey("utm_campaign") ? query["utm_campaign"].ToString() : string.Empty,
                UtmTerm = query.ContainsKey("utm_term") ? query["utm_term"].ToString() : string.Empty,
                UtmContent = query.ContainsKey("utm_content") ? query["utm_content"].ToString() : string.Empty,
                UserAgent = Request.Headers["User-Agent"].ToString(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                SessionId = HttpContext.Session.Id // Добавляем SessionId
            };

            if (!string.IsNullOrEmpty(utmData.UtmSource) || !string.IsNullOrEmpty(utmData.UtmCampaign))
            {
                try
                {
                    HttpContext.Session.SetString("UtmData", JsonSerializer.Serialize(utmData));
                    _logger.LogInformation("CampaignTracking: {@UtmData}", utmData);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error logging CampaignTracking: {Error}", ex.Message);
                }
            }

            return RedirectToPage("/Feed");
        }
    }
}