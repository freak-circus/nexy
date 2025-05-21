using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Nexy.Data;
using System;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Nexy.Pages.Model
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(ApplicationDbContext context, ILogger<DetailsModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public ModelProfile? Profile { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                Profile = await _context.ModelProfiles
                    .Include(m => m.Posts)
                    .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);

                if (Profile == null)
                    return NotFound();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in OnGetAsync: {Error}", ex.Message);
                return StatusCode(500);
            }
        }

        public IActionResult OnPostTrackClickAsync([FromBody] TrackClickData data)
        {
            try
            {
                _logger.LogInformation("TrackClick handler called with data: {@TrackClickData}", data);

                if (data == null || string.IsNullOrEmpty(data.Action) || data.ProfileId == Guid.Empty)
                {
                    _logger.LogWarning("Invalid TrackClick data: {@TrackClickData}", data);
                    return BadRequest();
                }

                // Проверка дублирования кликов
                var clickKey = $"Click_{data.ProfileId}_{HttpContext.Session.Id}";
                var isDuplicate = HttpContext.Session.GetString(clickKey) != null;
                if (isDuplicate)
                {
                    _logger.LogInformation("Duplicate click detected for ProfileId: {ProfileId}, SessionId: {SessionId}", data.ProfileId, HttpContext.Session.Id);
                    return new JsonResult(new { success = true, isDuplicate = true });
                }

                // Extract UTM parameters from session
                var utmData = HttpContext.Session.GetString("UtmData");
                _logger.LogInformation("UtmData from session: {UtmData}", utmData ?? "null");
                var campaignData = new
                {
                    Timestamp = DateTime.UtcNow.ToString("o"),
                    ProfileId = data.ProfileId,
                    Action = data.Action,
                    UtmSource = string.Empty,
                    UtmMedium = string.Empty,
                    UtmCampaign = string.Empty,
                    UtmTerm = string.Empty,
                    UtmContent = string.Empty,
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    SessionId = HttpContext.Session.Id
                };

                if (!string.IsNullOrEmpty(utmData))
                {
                    try
                    {
                        var utm = JsonSerializer.Deserialize<Dictionary<string, string>>(utmData);
                        campaignData = new
                        {
                            Timestamp = DateTime.UtcNow.ToString("o"),
                            ProfileId = data.ProfileId,
                            Action = data.Action,
                            UtmSource = utm.GetValueOrDefault("utm_source", string.Empty),
                            UtmMedium = utm.GetValueOrDefault("utm_medium", string.Empty),
                            UtmCampaign = utm.GetValueOrDefault("utm_campaign", string.Empty),
                            UtmTerm = utm.GetValueOrDefault("utm_term", string.Empty),
                            UtmContent = utm.GetValueOrDefault("utm_content", string.Empty),
                            UserAgent = Request.Headers["User-Agent"].ToString(),
                            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                            SessionId = utm.GetValueOrDefault("SessionId", HttpContext.Session.Id)
                        };
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning("Failed to deserialize UTM data from session: {Error}", ex.Message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Unexpected error deserializing UTM data: {Error}", ex.Message);
                    }
                }
                else
                {
                    _logger.LogWarning("UtmData not found in session for SessionId: {SessionId}", HttpContext.Session.Id);
                }

                // Логируем конверсию и сохраняем метку клика
                HttpContext.Session.SetString(clickKey, "tracked");
                _logger.LogInformation("ConversionTracking: {@ConversionData}", campaignData);
                return new JsonResult(new { success = true, isDuplicate = false });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in OnPostTrackClickAsync: {Error}", ex.Message);
                return StatusCode(500);
            }
        }

        public class TrackClickData
        {
            public Guid ProfileId { get; set; }
            public string Action { get; set; }
        }
    }
}