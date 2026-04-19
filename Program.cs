using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using STEMwise.Orchestrator.Data;
using STEMwise.Orchestrator.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        // Database Configuration - Robust Retrieval with Resiliency
        services.AddDbContext<OrchestratorContext>((sp, options) => {
            var config = sp.GetRequiredService<IConfiguration>();
            var connString = config.GetConnectionString("OrchestratorDb") 
                          ?? config["OrchestratorDb"]
                          ?? config["Values:OrchestratorDb"];
            
            options.UseSqlServer(connString, sqlOptions => {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(60);
            });
        });

        // Registers Typed HttpClients for APIs
        services.AddHttpClient<IScorecardService, ScorecardService>();
        services.AddHttpClient<IHudService, HudService>();
        services.AddHttpClient(); // Generic client for Hipo API

        // Registers Standard Services
        services.AddScoped<ILaborSyncService, LaborSyncService>();
        services.AddScoped<IGlobalSyncService, GlobalSyncService>();
        services.AddScoped<GlobalUniversityHarvester>();
    })
    .Build();

host.Run();
