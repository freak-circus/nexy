using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Nexy.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(ILogger<DashboardModel> logger)
        {
            _logger = logger;
        }

        public List<CampaignStats> CampaignStats { get; set; } = new();
        public List<ProfileStats> ProfileStats { get; set; } = new();
        public int TotalVisits { get; set; }
        public int TotalConversions { get; set; }
        public double OverallConversionRate { get; set; }

        public IActionResult OnGet()
        {
            try
            {
                // Чтение логов
                var logFile = $"logs/campaigns{DateTime.UtcNow:yyyyMMdd}.json";
                if (!System.IO.File.Exists(logFile))
                {
                    _logger.LogWarning("Log file not found: {LogFile}", logFile);
                    return Page();
                }

                var logs = new List<Dictionary<string, object>>();
                foreach (var line in System.IO.File.ReadAllLines(logFile))
                {
                    try
                    {
                        var log = JsonSerializer.Deserialize<Dictionary<string, object>>(line);
                        logs.Add(log);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning("Failed to parse log line: {Error}", ex.Message);
                    }
                }

                // Извлечение CampaignTracking и ConversionTracking
                var campaigns = logs
                    .Where(l => l["MessageTemplate"].ToString().StartsWith("CampaignTracking"))
                    .Select(l => {
                        var props = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(l["Properties"].ToString());
                        return JsonSerializer.Deserialize<Dictionary<string, string>>(props["UtmData"].ToString());
                    })
                    .ToList();

                var conversions = logs
                    .Where(l => l["MessageTemplate"].ToString().StartsWith("ConversionTracking"))
                    .Select(l => {
                        var props = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(l["Properties"].ToString());
                        return JsonSerializer.Deserialize<Dictionary<string, string>>(props["ConversionData"].ToString());
                    })
                    .ToList();

                _logger.LogInformation("Extracted {CampaignCount} CampaignTracking and {ConversionCount} ConversionTracking entries", campaigns.Count, conversions.Count);

                // Уникальные визиты по SessionId
                var uniqueVisits = campaigns
                    .GroupBy(c => c["SessionId"])
                    .Select(g => g.First())
                    .ToList();

                // Статистика по кампаниям
                var campaignGroups = uniqueVisits
                    .GroupBy(c => c.ContainsKey("UtmCampaign") && !string.IsNullOrEmpty(c["UtmCampaign"]) ? c["UtmCampaign"] : "Unknown")
                    .Select(g => {
                        var campaignConversions = conversions
                            .Count(c => c.ContainsKey("SessionId") && uniqueVisits.Any(v => v["SessionId"] == c["SessionId"] && v["UtmCampaign"] == g.Key));
                        return new CampaignStats
                        {
                            Campaign = g.Key,
                            Visits = g.Count(),
                            Conversions = campaignConversions,
                            ConversionRate = g.Count() > 0 ? Math.Round((double)campaignConversions / g.Count() * 100, 2) : 0
                        };
                    })
                    .OrderByDescending(c => c.ConversionRate)
                    .ToList();

                // Статистика по профилям
                var profileGroups = conversions
                    .GroupBy(c => c.ContainsKey("ProfileId") && !string.IsNullOrEmpty(c["ProfileId"]) ? c["ProfileId"] : "Unknown")
                    .Select(g => new ProfileStats
                    {
                        ProfileId = g.Key,
                        Conversions = g.Count()
                    })
                    .OrderByDescending(p => p.Conversions)
                    .ToList();

                // Общие метрики
                TotalVisits = uniqueVisits.Count;
                TotalConversions = conversions.Count;
                OverallConversionRate = TotalVisits > 0 ? Math.Round((double)TotalConversions / TotalVisits * 100, 2) : 0;

                CampaignStats = campaignGroups;
                ProfileStats = profileGroups;

                _logger.LogInformation("Dashboard loaded: {TotalVisits} visits, {TotalConversions} conversions, {CampaignCount} campaigns", TotalVisits, TotalConversions, CampaignStats.Count);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error loading dashboard: {Error}", ex.Message);
                return StatusCode(500);
            }
        }
    }

    public class CampaignStats
    {
        public string Campaign { get; set; }
        public int Visits { get; set; }
        public int Conversions { get; set; }
        public double ConversionRate { get; set; }
    }

    public class ProfileStats
    {
        public string ProfileId { get; set; }
        public int Conversions { get; set; }
    }
}