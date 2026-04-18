using System;

namespace STEMwise.Orchestrator.Models;

public class GlobalUniversityMetric
{
    public int Id { get; set; }
    public string CountryCode { get; set; } = string.Empty; // ISO 2-letter (UK, AU, DE, JP, CH)
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    
    // Outcome Benchmarks (Derived from 2024 official stats)
    public int? AnnualTuition { get; set; } // International tuition (local currency)
    public int? MedianSalary { get; set; } // Starting salary (local currency)
    public string Currency { get; set; } = "USD";
    
    public decimal? EmploymentRate { get; set; }
    public decimal? VisaSuccessRate { get; set; }
    public int? RoiScore { get; set; }
    
    public DateTime LastSynced { get; set; }
}
