using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using STEMwise.Orchestrator.Services;
using STEMwise.Orchestrator.Data;
using Microsoft.Extensions.DependencyInjection;

namespace STEMwise.Orchestrator;

public class OrchestratorFunction
{
    private readonly ILogger<OrchestratorFunction> _logger;
    private readonly IServiceProvider _serviceProvider;

    public OrchestratorFunction(ILogger<OrchestratorFunction> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    [Function("OrchestrateResearchData")]
    public async Task Run([TimerTrigger("0 0 0 * * 0")] TimerInfo myTimer)
    {
        _logger.LogInformation("Orchestrator Triggered: Starting Sync Cycle at: {time}", DateTime.Now);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OrchestratorContext>();
            var scorecardService = scope.ServiceProvider.GetRequiredService<IScorecardService>();
            var hudService = scope.ServiceProvider.GetRequiredService<IHudService>();
            var laborService = scope.ServiceProvider.GetRequiredService<ILaborSyncService>();
            var globalService = scope.ServiceProvider.GetRequiredService<IGlobalSyncService>();

            // Diagnostic Connection Check
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var connStr = config.GetConnectionString("OrchestratorDb") ?? config["OrchestratorDb"];
            
            if (string.IsNullOrEmpty(connStr))
            {
                _logger.LogError("CRITICAL: OrchestratorDb connection string is MISSING. Please check App Settings.");
                return;
            }
            else 
            {
                // Mask password for safe logging
                try {
                    var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connStr);
                    _logger.LogInformation("DIAGNOSTIC - Server: {srv} | DB: {db} | User: {usr}", 
                        builder.DataSource, builder.InitialCatalog, builder.UserID);
                } catch {
                    _logger.LogInformation("Connection string found (Length: {len}). Format is non-standard.", connStr.Length);
                }
            }

            // Ensure the database schema is up-to-date
            _logger.LogInformation("Verifying database schema...");
            await context.Database.EnsureCreatedAsync();

            // Execute Parallel Syncs
            _logger.LogInformation("Starting parallel sync jobs...");
            await Task.WhenAll(
                scorecardService.SyncRegionalUniversitiesAsync(),
                hudService.SyncRegionalRentsAsync(),
                laborService.SyncVisaBenchmarksAsync(),
                laborService.SyncSalaryBenchmarksAsync(),
                globalService.SyncGlobalBenchmarksAsync()
            );

            _logger.LogInformation("Sync Cycle Complete. Next occurrence: {next}", myTimer.ScheduleStatus?.Next);
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            _logger.LogError(ex, "CRITICAL ERROR in Sync: {msg}", msg);
            throw; 
        }
    }
}
