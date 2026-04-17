using STEMwise.Orchestrator.Services;
using STEMwise.Orchestrator.Data;

namespace STEMwise.Orchestrator;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Orchestrator Heartbeat: Starting Sync Cycle at: {time}", DateTimeOffset.Now);
            }

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<OrchestratorContext>();
                    var scorecardService = scope.ServiceProvider.GetRequiredService<IScorecardService>();
                    var hudService = scope.ServiceProvider.GetRequiredService<IHudService>();
                    var laborService = scope.ServiceProvider.GetRequiredService<ILaborSyncService>();

                    // Ensure the database and tables exist
                    _logger.LogInformation("Checking database schema...");
                    await context.Database.EnsureCreatedAsync(stoppingToken);

                    // Trigger Parallel Syncs for all data categories
                    _logger.LogInformation("Starting parallel sync jobs...");
                    await Task.WhenAll(
                        scorecardService.SyncRegionalUniversitiesAsync(),
                        hudService.SyncRegionalRentsAsync(),
                        laborService.SyncVisaBenchmarksAsync(),
                        laborService.SyncSalaryBenchmarksAsync()
                    );
                }

                _logger.LogInformation("Sync Cycle Complete. Sleeping for 24 hours...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A critical error occurred during the sync cycle.");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
