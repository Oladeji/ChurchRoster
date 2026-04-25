namespace ChurchRoster.Infrastructure.Services.Proposals;

/// <summary>
/// Strategy interface for roster proposal generation algorithms.
/// Implementations: <see cref="GreedyProposalService"/>, <see cref="OrToolsProposalService"/>.
/// </summary>
public interface IProposalGenerationStrategy
{
    /// <summary>
    /// Generates proposal items for the given proposal and persists them.
    /// Sets proposal.Status = Draft on success, Archived on failure.
    /// </summary>
    Task GenerateAsync(int proposalId, CancellationToken cancellationToken = default);
}
