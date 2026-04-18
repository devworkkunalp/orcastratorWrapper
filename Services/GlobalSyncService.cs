using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using STEMwise.Orchestrator.Data;
using STEMwise.Orchestrator.Models;

namespace STEMwise.Orchestrator.Services;

public interface IGlobalSyncService
{
    Task SyncGlobalBenchmarksAsync();
}

public class GlobalSyncService : IGlobalSyncService
{
    private readonly ILogger<GlobalSyncService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public GlobalSyncService(ILogger<GlobalSyncService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task SyncGlobalBenchmarksAsync()
    {
        _logger.LogInformation("Starting Global Benchmark Sync (UK, AU, DE, JP, CH)...");

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrchestratorContext>();

        var benchmarkData = GetSeededBenchmarks();

        foreach (var item in benchmarkData)
        {
            try
            {
                var existing = await context.GlobalUniversityMetrics
                    .FirstOrDefaultAsync(g => g.CountryCode == item.CountryCode && g.Name == item.Name);

                if (existing == null)
                {
                    existing = new GlobalUniversityMetric
                    {
                        CountryCode = item.CountryCode,
                        Name = item.Name,
                        City = item.City
                    };
                    context.GlobalUniversityMetrics.Add(existing);
                }

                existing.AnnualTuition = item.AnnualTuition;
                existing.MedianSalary = item.MedianSalary;
                existing.Currency = item.Currency;
                existing.EmploymentRate = item.EmploymentRate;
                existing.VisaSuccessRate = item.VisaSuccessRate;
                existing.RoiScore = item.RoiScore;
                existing.LastSynced = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync global benchmark for {UniName} in {Country}", item.Name, item.CountryCode);
            }
        }

        await context.SaveChangesAsync();
        _logger.LogInformation("Global Benchmark Sync Complete.");
    }

    private List<GlobalUniversityMetric> GetSeededBenchmarks()
    {
        return new List<GlobalUniversityMetric>
        {
            // --- UNITED KINGDOM (GBP) ---
            new() { CountryCode = "UK", Name = "University of Oxford", City = "Oxford", AnnualTuition = 42000, MedianSalary = 48000, Currency = "GBP", EmploymentRate = 0.94m, VisaSuccessRate = 0.85m, RoiScore = 88 },
            new() { CountryCode = "UK", Name = "University of Cambridge", City = "Cambridge", AnnualTuition = 40000, MedianSalary = 47000, Currency = "GBP", EmploymentRate = 0.93m, VisaSuccessRate = 0.84m, RoiScore = 87 },
            new() { CountryCode = "UK", Name = "Imperial College London", City = "London", AnnualTuition = 38000, MedianSalary = 52000, Currency = "GBP", EmploymentRate = 0.96m, VisaSuccessRate = 0.88m, RoiScore = 92 },
            new() { CountryCode = "UK", Name = "UCL (Univ. College London)", City = "London", AnnualTuition = 34000, MedianSalary = 45000, Currency = "GBP", EmploymentRate = 0.92m, VisaSuccessRate = 0.82m, RoiScore = 85 },
            
            // --- AUSTRALIA (AUD) ---
            new() { CountryCode = "AU", Name = "University of Melbourne", City = "Melbourne", AnnualTuition = 48000, MedianSalary = 78000, Currency = "AUD", EmploymentRate = 0.90m, VisaSuccessRate = 0.75m, RoiScore = 82 },
            new() { CountryCode = "AU", Name = "University of Sydney", City = "Sydney", AnnualTuition = 52000, MedianSalary = 82000, Currency = "AUD", EmploymentRate = 0.91m, VisaSuccessRate = 0.74m, RoiScore = 81 },
            new() { CountryCode = "AU", Name = "UNSW Sydney", City = "Sydney", AnnualTuition = 45000, MedianSalary = 85000, Currency = "AUD", EmploymentRate = 0.93m, VisaSuccessRate = 0.78m, RoiScore = 86 },
            new() { CountryCode = "AU", Name = "Monash University", City = "Melbourne", AnnualTuition = 42000, MedianSalary = 75000, Currency = "AUD", EmploymentRate = 0.89m, VisaSuccessRate = 0.72m, RoiScore = 79 },

            // --- GERMANY (EUR) ---
            new() { CountryCode = "DE", Name = "Technical University of Munich (TUM)", City = "Munich", AnnualTuition = 6000, MedianSalary = 62000, Currency = "EUR", EmploymentRate = 0.95m, VisaSuccessRate = 0.92m, RoiScore = 96 },
            new() { CountryCode = "DE", Name = "RWTH Aachen University", City = "Aachen", AnnualTuition = 3000, MedianSalary = 58000, Currency = "EUR", EmploymentRate = 0.94m, VisaSuccessRate = 0.90m, RoiScore = 95 },
            new() { CountryCode = "DE", Name = "Karlsruhe Inst. of Tech (KIT)", City = "Karlsruhe", AnnualTuition = 3000, MedianSalary = 60000, Currency = "EUR", EmploymentRate = 0.94m, VisaSuccessRate = 0.89m, RoiScore = 94 },

            // --- JAPAN (JPY) ---
            new() { CountryCode = "JP", Name = "University of Tokyo", City = "Tokyo", AnnualTuition = 820000, MedianSalary = 5500000, Currency = "JPY", EmploymentRate = 0.96m, VisaSuccessRate = 0.80m, RoiScore = 84 },
            new() { CountryCode = "JP", Name = "Kyoto University", City = "Kyoto", AnnualTuition = 820000, MedianSalary = 5200000, Currency = "JPY", EmploymentRate = 0.94m, VisaSuccessRate = 0.78m, RoiScore = 81 },
            new() { CountryCode = "JP", Name = "Tokyo Institute of Technology", City = "Tokyo", AnnualTuition = 820000, MedianSalary = 5800000, Currency = "JPY", EmploymentRate = 0.97m, VisaSuccessRate = 0.82m, RoiScore = 88 },

            // --- SWITZERLAND (CHF) ---
            new() { CountryCode = "CH", Name = "ETH Zurich", City = "Zurich", AnnualTuition = 1500, MedianSalary = 95000, Currency = "CHF", EmploymentRate = 0.98m, VisaSuccessRate = 0.90m, RoiScore = 99 },
            new() { CountryCode = "CH", Name = "EPFL", City = "Lausanne", AnnualTuition = 1500, MedianSalary = 92000, Currency = "CHF", EmploymentRate = 0.97m, VisaSuccessRate = 0.88m, RoiScore = 98 },
            new() { CountryCode = "CH", Name = "University of Zurich", City = "Zurich", AnnualTuition = 2000, MedianSalary = 85000, Currency = "CHF", EmploymentRate = 0.94m, VisaSuccessRate = 0.85m, RoiScore = 92 }
        };
    }
}
