using System;

namespace STEMwise.Orchestrator.Models;

public class VisaBenchmark
{
    public int Id { get; set; }
    public string RegionName { get; set; } = string.Empty; // e.g. Silicon Valley, Boston Metro
    
    public int TotalPetitions { get; set; }
    public int Approvals { get; set; }
    public int Denials { get; set; }
    public decimal SuccessRate => TotalPetitions > 0 ? (decimal)Approvals / TotalPetitions : 0;
    
    public int FiscalYear { get; set; }
    public DateTime LastSynced { get; set; }
}

public class LaborBenchmark
{
    public int Id { get; set; }
    public string RegionName { get; set; } = string.Empty; // e.g. Silicon Valley, Boston Metro
    
    public int JobCount { get; set; }
    public int AvgSalary { get; set; }
    public int MedianSalary { get; set; }
    public int Percentile75Salary { get; set; }
    
    public DateTime LastSynced { get; set; }
}
