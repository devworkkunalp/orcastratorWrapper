using Microsoft.EntityFrameworkCore;
using STEMwise.Orchestrator.Models;

namespace STEMwise.Orchestrator.Data;

public class OrchestratorContext : DbContext
{
    public OrchestratorContext(DbContextOptions<OrchestratorContext> options)
        : base(options)
    {
    }

    public DbSet<UniversityMetric> UniversityMetrics { get; set; } = null!;
    public DbSet<RegionalRent> RegionalRents { get; set; } = null!;
    public DbSet<VisaBenchmark> VisaBenchmarks { get; set; } = null!;
    public DbSet<LaborBenchmark> LaborBenchmarks { get; set; } = null!;
    public DbSet<GlobalUniversityMetric> GlobalUniversityMetrics { get; set; } = null!;
    public DbSet<GlobalSectorBenchmark> GlobalSectorBenchmarks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique mapping to ensure 1-record-per-entity (Lean Data Policy)
        modelBuilder.Entity<UniversityMetric>().HasIndex(u => u.UnitId).IsUnique();
        modelBuilder.Entity<RegionalRent>().HasIndex(r => r.MsaId).IsUnique();
        
        // Multi-column index for Specialization support
        modelBuilder.Entity<VisaBenchmark>().HasIndex(v => new { v.RegionName, v.Specialization }).IsUnique();
        modelBuilder.Entity<LaborBenchmark>().HasIndex(l => new { l.RegionName, l.Specialization }).IsUnique();
        modelBuilder.Entity<GlobalUniversityMetric>().HasIndex(g => new { g.CountryCode, g.Name }).IsUnique();
        modelBuilder.Entity<GlobalSectorBenchmark>().HasIndex(gs => new { gs.CountryCode, gs.Specialization }).IsUnique();
    }
}
