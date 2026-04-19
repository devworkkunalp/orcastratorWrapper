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
    public string CountryCode { get; set; } = "US"; // Default for backward compatibility
    
    // Global Performance Indices
    public int? PrEaseScore { get; set; } // 0-100: Policy ease for International Students
    public int? QoLIndex { get; set; } // 0-100: Quality of Life benchmarking
    
    // Academic ROI Aggregates
    public int? MedianEarnings { get; set; }
    public int? MedianDebt { get; set; }
    public int? AnnualTuition { get; set; }
    public decimal? GraduationRate { get; set; }
    public decimal? AdmissionRate { get; set; }
    
    // Derived Research Hub Metrics
    public int? RoiScore { get; set; }
    public decimal? EmploymentRate { get; set; }
    public decimal? IntlStudentShare { get; set; } // H-1B Proxy
    
    public DateTime LastSynced { get; set; }
    public string? ProgramDataJson { get; set; }
}
