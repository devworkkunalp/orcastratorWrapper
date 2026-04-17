using System;

namespace STEMwise.Orchestrator.Models;

public class UniversityMetric
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int UnitId { get; set; } // Primary identifier from College Scorecard API
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZIP { get; set; } = string.Empty;
    
    // Academic ROI Aggregates
    public int? MedianEarnings { get; set; }
    public int? MedianDebt { get; set; }
    public decimal? GraduationRate { get; set; }
    public decimal? AdmissionRate { get; set; }
    
    // Derived Research Hub Metrics
    public int? RoiScore { get; set; }
    public decimal? EmploymentRate { get; set; }
    
    public DateTime LastSynced { get; set; }
}
