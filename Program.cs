using Microsoft.EntityFrameworkCore;
using STEMwise.Orchestrator;
using STEMwise.Orchestrator.Data;
using STEMwise.Orchestrator.Services;

var builder = Host.CreateApplicationBuilder(args);

// Database Configuration
builder.Services.AddDbContext<OrchestratorContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrchestratorDb")));

// Registers Typed HttpClients for APIs
builder.Services.AddHttpClient<IScorecardService, ScorecardService>();
builder.Services.AddHttpClient<IHudService, HudService>();

// Registers Standard Services
builder.Services.AddScoped<ILaborSyncService, LaborSyncService>();

// Register Background Sync Worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
