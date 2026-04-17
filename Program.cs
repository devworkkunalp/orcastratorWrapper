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
        // Database Configuration
        services.AddDbContext<OrchestratorContext>(options =>
            options.UseSqlServer(context.Configuration.GetConnectionString("OrchestratorDb")));

        // Registers Typed HttpClients for APIs
        services.AddHttpClient<IScorecardService, ScorecardService>();
        services.AddHttpClient<IHudService, HudService>();

        // Registers Standard Services
        services.AddScoped<ILaborSyncService, LaborSyncService>();
    })
    .Build();

host.Run();
