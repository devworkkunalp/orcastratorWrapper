using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using STEMwise.Orchestrator.Data;
using STEMwise.Orchestrator.Models;

namespace STEMwise.Orchestrator.Services;

public class GlobalBenchmarkSeeder
{
    private readonly OrchestratorContext _context;

    public GlobalBenchmarkSeeder(OrchestratorContext context)
    {
        _context = context;
    }

    private string NormalizeSpecialization(string? specialization)
    {
        if (string.IsNullOrEmpty(specialization)) return "General";
        return specialization.ToLower() switch {
            "cs" => "Computer Science / AI",
            "computer science / ai" => "Computer Science / AI",
            "cyber" => "Cybersecurity",
            "cybersecurity" => "Cybersecurity",
            "data" => "Data Science / Analytics",
            "data science / analytics" => "Data Science / Analytics",
            "electrical" => "Electrical Engineering",
            "electrical engineering" => "Electrical Engineering",
            "biomedical" => "Biomedical Sciences",
            "mechanical" => "Mechanical Engineering",
            "mechanical engineering" => "Mechanical Engineering",
            _ => specialization
        };
    }

    public async Task SeedBenchmarksAsync()
    {
        Console.WriteLine("[SEED] Starting Global Benchmark Seeding...");

        var countries = new[] { "CA", "DE", "GB", "AU" };
        var specializations = new[] { "Computer Science / AI", "Data Science", "Cybersecurity", "Electrical Engineering", "Mechanical Engineering", "Biomedical" };

        foreach (var country in countries)
        {
            foreach (var rawSpec in specializations)
            {
                var spec = NormalizeSpecialization(rawSpec);

                // 1. Seed Labor Benchmarks
                var regionName = GetRepresentativeHub(country);
                var laborExists = await _context.LaborBenchmarks.AnyAsync(l => l.RegionName == regionName && l.Specialization == spec);
                if (!laborExists)
                {
                    _context.LaborBenchmarks.Add(new LaborBenchmark
                    {
                        CountryCode = country,
                        Specialization = spec,
                        RegionName = regionName,
                        MedianSalary = GetEstimatedSalary(country, rawSpec),
                        Percentile10Salary = GetEstimatedSalary(country, rawSpec, 0.6),
                        Percentile25Salary = GetEstimatedSalary(country, rawSpec, 0.8),
                        Percentile75Salary = GetEstimatedSalary(country, rawSpec, 1.25),
                        Percentile90Salary = GetEstimatedSalary(country, rawSpec, 1.5),
                        RentMedian = GetEstimatedRent(country),
                        LastSynced = DateTime.UtcNow
                    });
                }

                // 2. Seed Global Sector Benchmarks (For the comparison table)
                var sectorExists = await _context.GlobalSectorBenchmarks.AnyAsync(s => s.CountryCode == country && s.Specialization == spec);
                if (!sectorExists)
                {
                    var countryName = GetCountryName(country);
                    _context.GlobalSectorBenchmarks.Add(new GlobalSectorBenchmark
                    {
                        CountryCode = country,
                        CountryName = countryName,
                        Flag = GetFlag(country),
                        Specialization = spec,
                        MedianSalary = GetEstimatedSalary(country, rawSpec),
                        PrMetric = GetPrMetric(country),
                        VisaEase = GetVisaEase(country),
                        RoiScore = GetRoiScore(country),
                        LastSynced = DateTime.UtcNow
                    });
                }
            }
        }

        await _context.SaveChangesAsync();
        Console.WriteLine("[SEED] Global Benchmarks Seeded Successfully.");
    }

    private string GetRepresentativeHub(string code) => code switch
    {
        "CA" => "Toronto Metro",
        "DE" => "Munich / Berlin",
        "GB" => "London / Manchester",
        "AU" => "Sydney Metro",
        _ => "General"
    };

    private int GetEstimatedSalary(string country, string spec, double multiplier = 1.0)
    {
        int baseSalary = spec switch
        {
            "Computer Science / AI" => 110000,
            "Data Science" => 105000,
            "Cybersecurity" => 100000,
            "Electrical Engineering" => 95000,
            "Mechanical Engineering" => 85000,
            "Biomedical" => 90000,
            _ => 80000
        };

        double countryFactor = country switch
        {
            "US" => 1.0,
            "CA" => 0.85,  // CAD roughly lower than USD base
            "GB" => 0.65,  // GBP base
            "DE" => 0.70,  // EUR base
            "AU" => 0.90,  // AUD base
            _ => 1.0
        };

        return (int)(baseSalary * countryFactor * multiplier);
    }

    private string GetPrEaseMetric(string country) => country switch
    {
        "CA" => "Guaranteed (Express Entry)",
        "DE" => "Smooth (Blue Card)",
        "AU" => "Points-Based",
        "GB" => "Merit-Based",
        "US" => "Lottery System",
        _ => "Standard"
    };

    private string GetVisaEaseMetric(string country) => country switch
    {
        "CA" => "Very High (95%)",
        "DE" => "High (85%)",
        "AU" => "High (82%)",
        "GB" => "Moderate (70%)",
        "US" => "Low (45%)",
        _ => "Moderate"
    };

    private int GetDefaultRoiScore(string country, string spec)
    {
        // Simple heuristic for ROI: lower tuition + high PR = higher ROI
        int score = 70; 
        if (country == "CA" || country == "DE") score += 15;
        if (spec.Contains("AI") || spec.Contains("Data")) score += 10;
        return Math.Min(score, 99);
    }

    private string GetCountryName(string code) => code switch
    {
        "CA" => "Canada", "DE" => "Germany", "GB" => "United Kingdom", "AU" => "Australia", "US" => "United States", _ => code
    };

    private string GetFlag(string code) => code switch
    {
        "CA" => "🇨🇦", "DE" => "🇩🇪", "GB" => "🇬🇧", "AU" => "🇦🇺", "US" => "🇺🇸", _ => "🌐"
    };
}
