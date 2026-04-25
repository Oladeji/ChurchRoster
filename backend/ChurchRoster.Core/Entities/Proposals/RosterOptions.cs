namespace ChurchRoster.Core.Entities.Proposals;

/// <summary>
/// Controls which roster generation algorithm is used.
/// Bound from the "RosterGeneration" config section in appsettings.
/// </summary>
public class RosterOptions
{
    /// <summary>
    /// Algorithm to use for proposal generation.
    /// Accepted values (case-insensitive): "Greedy", "OrTools"
    /// Default: "Greedy"
    /// </summary>
    public string Algorithm { get; set; } = "Greedy";
}
