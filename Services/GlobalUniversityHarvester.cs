using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using STEMwise.Orchestrator.Data;
using STEMwise.Orchestrator.Models;

namespace STEMwise.Orchestrator.Services;

public class GlobalUniversityHarvester
{
    private readonly OrchestratorContext _context;
    private readonly HttpClient _httpClient;

    public GlobalUniversityHarvester(OrchestratorContext context, HttpClient httpClient)
    {
        _context = context;
        _httpClient = httpClient;
    }

    public async Task HarvestFocusCountriesAsync()
    {
        var countries = new Dictionary<string, string>
        {
            { "United States", "US" },
            { "United Kingdom", "GB" },
            { "Canada", "CA" },
            { "Germany", "DE" },
            { "Australia", "AU" }
        };

        foreach (var country in countries)
        {
            await HarvestCountryAsync(country.Key, country.Value);
        }
    }

    public async Task HarvestCountryAsync(string countryName, string countryCode)
    {
        try
        {
            Console.WriteLine($"[HARVEST] Fetching universities for {countryName} ({countryCode})...");
            var response = await _httpClient.GetFromJsonAsync<List<HipoUniversity>>($"http://universities.hipo.com/search?country={countryName}");

            if (response == null || !response.Any()) return;

            int newCount = 0;
            foreach (var hUni in response)
            {
                var exists = await _context.UniversityMetrics
                    .AnyAsync(u => u.Name == hUni.Name && u.CountryCode == countryCode);

                if (!exists)
                {
                    var uni = new UniversityMetric
                    {
                        Name = hUni.Name,
                        CountryCode = countryCode,
                        City = hUni.StateProvince ?? "N/A",
                        State = hUni.StateProvince ?? "",
                        LastSynced = DateTime.UtcNow,
                        // Apply Default Tier-Based Benchmarks (Simple Model)
                        AnnualTuition = GetDefaultTuition(countryCode),
                        PrEaseScore = GetDefaultPrScore(countryCode),
                        QoLIndex = GetDefaultQolIndex(countryCode),
                        RoiScore = 50 // Baseline
                    };

                    _context.UniversityMetrics.Add(uni);
                    newCount++;
                }
            }

            await _context.SaveChangesAsync();
            Console.WriteLine($"[HARVEST] Added {newCount} new universities for {countryName}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HARVEST ERROR] Failed to harvest {countryName}: {ex.Message}");
        }
    }

    private int GetDefaultTuition(string countryCode) => countryCode switch
    {
        "US" => 45000,
        "CA" => 35000,
        "GB" => 25000,
        "AU" => 38000,
        "DE" => 3000,
        _ => 20000
    };

    private int GetDefaultPrScore(string countryCode) => countryCode switch
    {
        "CA" => 95,
        "DE" => 85,
        "AU" => 75,
        "GB" => 60,
        "US" => 35,
        _ => 50
    };

    private int GetDefaultQolIndex(string countryCode) => countryCode switch
    {
        "DE" => 92,
        "AU" => 90,
        "CA" => 88,
        "GB" => 82,
        "US" => 78,
        _ => 70
    };

    private class HipoUniversity
    {
        public string Name { get; set; } = string.Empty;
        public string? StateProvince { get; set; }
    }
}
