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

public interface ILaborSyncService
{
    Task SyncVisaBenchmarksAsync();
    Task SyncSalaryBenchmarksAsync();
}

public class LaborSyncService : ILaborSyncService
{
    private readonly ILogger<LaborSyncService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public LaborSyncService(ILogger<LaborSyncService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task SyncVisaBenchmarksAsync()
    {
        _logger.LogInformation("Syncing Visa Benchmarks with Regional Aggregation...");
        
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrchestratorContext>();

        // We target only our 6 Power Regions
        foreach (var metro in PowerRegions.TargetMetros)
        {
            try
            {
                // In a real scenario, this would parse a USCIS CSV stream filtering for the MSA/Metro name.
                // For the free tier DB, we store the result of the aggregation, NOT the raw petitions.
                
                var existing = await context.VisaBenchmarks
                    .FirstOrDefaultAsync(v => v.RegionName == metro.Name);

                if (existing == null)
                {
                    existing = new VisaBenchmark { RegionName = metro.Name };
                    context.VisaBenchmarks.Add(existing);
                }

                // Placeholder aggregated data (Simulating extraction from USCIS CSV)
                existing.TotalPetitions = new Random().Next(4000, 15000);
                existing.Approvals = (int)(existing.TotalPetitions * 0.92);
                existing.Denials = existing.TotalPetitions - existing.Approvals;
                existing.FiscalYear = DateTime.UtcNow.Year;
                existing.LastSynced = DateTime.UtcNow;

                _logger.LogInformation("Aggregated Visa Data for {Region}: {Count} petitions handled.", metro.Name, existing.TotalPetitions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error aggregating visa benchmarks for {Region}", metro.Name);
            }
        }

        await context.SaveChangesAsync();
    }

    public async Task SyncSalaryBenchmarksAsync()
    {
        _logger.LogInformation("Syncing Salary Benchmarks with Regional Aggregation...");
        
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrchestratorContext>();

        foreach (var metro in PowerRegions.TargetMetros)
        {
            try
            {
                // Simulating aggregation from DOL Certified LCAs
                var existing = await context.LaborBenchmarks
                    .FirstOrDefaultAsync(l => l.RegionName == metro.Name);

                if (existing == null)
                {
                    existing = new LaborBenchmark { RegionName = metro.Name };
                    context.LaborBenchmarks.Add(existing);
                }

                // Regional Aggregates (Keep only the percentiles to save space)
                existing.JobCount = new Random().Next(5000, 20000);
                existing.AvgSalary = new Random().Next(110000, 180000);
                existing.MedianSalary = existing.AvgSalary - 5000;
                existing.Percentile75Salary = existing.AvgSalary + 25000;
                
                existing.LastSynced = DateTime.UtcNow;

                _logger.LogInformation("Aggregated Labor Data for {Region}: Median Salary ${Median}", metro.Name, existing.MedianSalary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error aggregating labor benchmarks for {Region}", metro.Name);
            }
        }

        await context.SaveChangesAsync();
    }
}
