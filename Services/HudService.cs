using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using STEMwise.Orchestrator.Data;
using STEMwise.Orchestrator.Models;

namespace STEMwise.Orchestrator.Services;

public interface IHudService
{
    Task SyncRegionalRentsAsync();
}

public class HudService : IHudService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiToken;
    private readonly ILogger<HudService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public HudService(
        HttpClient httpClient, 
        IConfiguration configuration, 
        ILogger<HudService> logger,
        IServiceProvider serviceProvider)
    {
        _httpClient = httpClient;
        _apiToken = configuration["ExternalApis:HudUserToken"] ?? string.Empty;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task SyncRegionalRentsAsync()
    {
        _logger.LogInformation("Starting Regional Rent Sync with Lean Data Policy...");

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrchestratorContext>();

        foreach (var metro in PowerRegions.TargetMetros)
        {
            try
            {
                _logger.LogInformation("Processing rent benchmarks for {MetroName}", metro.Name);

                var existing = await context.RegionalRents
                    .FirstOrDefaultAsync(r => r.MsaId == metro.MsaId);

                if (existing == null)
                {
                    existing = new RegionalRent { MsaId = metro.MsaId, RegionName = metro.Name };
                    context.RegionalRents.Add(existing);
                }

                // Attempt HTTP call to HUD API
                var url = $"https://www.huduser.gov/portal/datasets/fmr/fmrapi/v3/info/{metro.MsaId}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                if (!string.IsNullOrEmpty(_apiToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiToken);
                }

                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<HudResponse>();
                    if (data?.Basic != null)
                    {
                        existing.EfficiencyRent = data.Basic.Efficiency;
                        existing.OneBedRent = data.Basic.OneBed;
                        existing.TwoBedRent = data.Basic.TwoBed;
                        _logger.LogInformation("Updated real rent data for {MetroName}", metro.Name);
                    }
                }
                else
                {
                    // Fallback to current market estimates if API is unreachable/blocked
                    _logger.LogWarning("HUD API returned {Status}. Using market fallback for {MetroName}.", response.StatusCode, metro.Name);
                    ApplyMarketFallback(existing, metro.Name);
                }

                existing.LastSynced = DateTime.UtcNow;
                existing.EffectiveYear = new DateTime(DateTime.UtcNow.Year, 1, 1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync rent for {MetroName}", metro.Name);
            }
        }

        await context.SaveChangesAsync();
        _logger.LogInformation("Regional Rent Sync Complete.");
    }

    private void ApplyMarketFallback(RegionalRent rent, string metroName)
    {
        // Average 2024/2025 benchmarks for target STEM hubs
        switch (metroName)
        {
            case "Silicon Valley": rent.OneBedRent = 2800; rent.TwoBedRent = 3400; break;
            case "Boston Metro": rent.OneBedRent = 2400; rent.TwoBedRent = 2900; break;
            case "NYC Metro": rent.OneBedRent = 2600; rent.TwoBedRent = 3100; break;
            case "Seattle Metro": rent.OneBedRent = 2100; rent.TwoBedRent = 2600; break;
            case "Austin Hills": rent.OneBedRent = 1600; rent.TwoBedRent = 2000; break;
            case "Atlanta Hub": rent.OneBedRent = 1500; rent.TwoBedRent = 1800; break;
            default: rent.OneBedRent = 1200; rent.TwoBedRent = 1500; break;
        }
    }

    private class HudResponse
    {
        public HudData? Basic { get; set; }
    }

    private class HudData
    {
        public int Efficiency { get; set; }
        public int OneBed { get; set; }
        public int TwoBed { get; set; }
    }
}
