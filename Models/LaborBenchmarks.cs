using System;

namespace STEMwise.Orchestrator.Models;

public class Specialization
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public string Category { get; set; } = "STEM";
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class VisaBenchmark
{
    public int Id { get; set; }
    public string RegionName { get; set; } = string.Empty; // e.g. Silicon Valley, Boston Metro
    public string CountryCode { get; set; } = "US";
    public int SpecializationId { get; set; }
    
    public int TotalPetitions { get; set; }
    public int Approvals { get; set; }
    public int Denials { get; set; }
    public decimal SuccessRate => TotalPetitions > 0 ? (decimal)Approvals / TotalPetitions : 0;
    
    // Performance Projections
    public int OutcomeEmployedPct { get; set; }
    public int OutcomeH1BPct { get; set; }
    public int OutcomeReturnedPct { get; set; }

    public int FiscalYear { get; set; }
    public DateTime LastSynced { get; set; }
}

public class LaborBenchmark
{
    public int Id { get; set; }
    public string RegionName { get; set; } = string.Empty; // e.g. Silicon Valley, Boston Metro
    public string CountryCode { get; set; } = "US";
    public int SpecializationId { get; set; }
    
    public int JobCount { get; set; }
    public int AvgSalary { get; set; }
    public int MedianSalary { get; set; }
    
    // Distribution metrics
    public int Percentile10Salary { get; set; }
    public int Percentile25Salary { get; set; }
    public int Percentile75Salary { get; set; }
    public int Percentile90Salary { get; set; }
    
    public int RentMedian { get; set; }
    
    public DateTime LastSynced { get; set; }
}

public class GlobalSectorBenchmark
{
    public int Id { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public string CountryCode { get; set; } = "US";
    public string Flag { get; set; } = string.Empty;
    public int SpecializationId { get; set; }
    
    public int MedianSalary { get; set; }
    public string PrMetric { get; set; } = string.Empty;
    public string VisaEase { get; set; } = string.Empty;
    public int RoiScore { get; set; }

    public DateTime LastSynced { get; set; }
}
