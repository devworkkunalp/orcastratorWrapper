using System;

namespace STEMwise.Orchestrator.Models;

public class RegionalRent
{
    public int Id { get; set; }
    public string RegionName { get; set; } = string.Empty;
    public string MsaId { get; set; } = string.Empty; // Metropolitan Statistical Area ID for HUD
    public string State { get; set; } = string.Empty;
    
    public int EfficiencyRent { get; set; }
    public int OneBedRent { get; set; }
    public int TwoBedRent { get; set; }
    
    public DateTime EffectiveYear { get; set; }
    public DateTime LastSynced { get; set; }
}

public static class PowerRegions
{
    public static readonly string[] TopStates = { "CA", "TX", "NY", "MA", "WA", "GA" };
    
    public static readonly (string Name, string MsaId)[] TargetMetros = {
        ("Silicon Valley", "METRO41940M41940"), // San Jose-Sunnyvale-Santa Clara
        ("Boston Metro", "METRO71650M71650"),  // Boston-Cambridge-Newton
        ("Austin Hills", "METRO12420M12420"),  // Austin-Round Rock
        ("NYC Metro", "METRO35620M35620"),     // New York-Newark-Jersey City
        ("Seattle Metro", "METRO42660M42660"), // Seattle-Tacoma-Bellevue
        ("Atlanta Hub", "METRO12060M12060")    // Atlanta-Sandy Springs-Alpharetta
    };
}
