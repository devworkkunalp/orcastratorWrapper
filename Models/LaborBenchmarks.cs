using System;

namespace STEMwise.Orchestrator.Models;

public class VisaBenchmark
{
    public int Id { get; set; }
    public string RegionName { get; set; } = string.Empty; // e.g. Silicon Valley, Boston Metro
    public string Specialization { get; set; } = "General";
    
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
    public string Specialization { get; set; } = "General";
    
    public int JobCount { get; set; }
    public int AvgSalary { get; set; }
    public int MedianSalary { get; set; }
    
    // Distribution metrics
    public int Percentile10Salary { get; set; }
    public int Percentile25Salary { get; set; }
    public int Percentile75Salary { get; set; }
    public int Percentile90Salary { get; set; }
    
    public DateTime LastSynced { get; set; }
}

public class GlobalSectorBenchmark
{
    public int Id { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public string Flag { get; set; } = string.Empty;
    public string Specialization { get; set; } = "General";
    
    public int MedianSalary { get; set; }
    public string PrMetric { get; set; } = string.Empty;
    public string VisaEase { get; set; } = string.Empty;
    public int RoiScore { get; set; }

    public DateTime LastSynced { get; set; }
}
