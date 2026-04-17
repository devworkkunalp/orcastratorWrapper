using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using STEMwise.Orchestrator.Data;
using STEMwise.Orchestrator.Models;

namespace STEMwise.Orchestrator.Services;

public interface IScorecardService
{
    Task SyncRegionalUniversitiesAsync();
}

public class ScorecardService : IScorecardService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<ScorecardService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ScorecardService(
        HttpClient httpClient, 
        IConfiguration configuration, 
        ILogger<ScorecardService> logger,
        IServiceProvider serviceProvider)
    {
        _httpClient = httpClient;
        _apiKey = configuration["ExternalApis:CollegeScorecardKey"] ?? string.Empty;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task SyncRegionalUniversitiesAsync()
    {
        _logger.LogInformation("Starting Regional University Sync with Lean Data Policy...");

        foreach (var state in PowerRegions.TopStates)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<OrchestratorContext>();

                _logger.LogInformation("Fetching target universities for state: {State}", state);
                
                var fields = "id,school.name,school.city,school.state,school.zip,latest.earnings.10_yrs_after_entry.median,latest.admissions.admission_rate,latest.completion.completion_rate_4yr_150nt";
                var url = $"https://api.data.gov/ed/collegescorecard/v1/schools.json?school.state={state}&api_key={_apiKey}&fields={fields}&per_page=100&sort=latest.earnings.10_yrs_after_entry.median:desc";
                
                var response = await _httpClient.GetFromJsonAsync<ScorecardRoot>(url);
                
                if (response?.Results != null)
                {
                    _logger.LogInformation("Processing {Count} schools for {State}", response.Results.Count, state);
                    
                    foreach (var item in response.Results)
                    {
                        var existing = await context.UniversityMetrics
                            .FirstOrDefaultAsync(u => u.UnitId == item.Id);

                        if (existing == null)
                        {
                            existing = new UniversityMetric { UnitId = item.Id };
                            context.UniversityMetrics.Add(existing);
                        }

                        // Update fields (Lean Policy: Overwrite with latest)
                        existing.Name = item.Name ?? "Unknown";
                        existing.City = item.City ?? "Unknown";
                        existing.State = item.State ?? state;
                        existing.ZIP = item.Zip ?? string.Empty;
                        existing.MedianEarnings = item.MedianEarnings;
                        existing.GraduationRate = (decimal?)item.GradRate;
                        existing.AdmissionRate = (decimal?)item.AdmitRate;
                        existing.LastSynced = DateTime.UtcNow;

                        // Basic ROI Score calculation placeholder (will refine in backend)
                        if (existing.MedianEarnings.HasValue)
                        {
                            existing.RoiScore = (int)((existing.MedianEarnings / 150000.0) * 100);
                        }
                    }

                    await context.SaveChangesAsync();
                    _logger.LogInformation("Successfully synced batch for {State}", state);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync universities for state {State}", state);
            }
        }
    }

    private class ScorecardRoot
    {
        [JsonPropertyName("results")]
        public List<ScorecardItem>? Results { get; set; }
    }

    private class ScorecardItem
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("school.name")] public string? Name { get; set; }
        [JsonPropertyName("school.city")] public string? City { get; set; }
        [JsonPropertyName("school.state")] public string? State { get; set; }
        [JsonPropertyName("school.zip")] public string? Zip { get; set; }
        
        [JsonPropertyName("latest.earnings.10_yrs_after_entry.median")] 
        public int? MedianEarnings { get; set; }
        
        [JsonPropertyName("latest.admissions.admission_rate")]
        public double? AdmitRate { get; set; }
        
        [JsonPropertyName("latest.completion.completion_rate_4yr_150nt")]
        public double? GradRate { get; set; }
    }
}
