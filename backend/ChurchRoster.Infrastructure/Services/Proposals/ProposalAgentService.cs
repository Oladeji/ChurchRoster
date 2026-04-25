using ChurchRoster.Core.Entities.Proposals;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChurchRoster.Infrastructure.Services.Proposals;

/// <summary>
/// Entry point for proposal generation, called by ProposalGenerationJob.
/// Reads RosterOptions.Algorithm from configuration and delegates to the matching strategy:
///   "Greedy"  => GreedyProposalService
///   "OrTools" => OrToolsProposalService
/// </summary>
public class ProposalAgentService
{
    private readonly IProposalGenerationStrategy _strategy;
    private readonly ILogger<ProposalAgentService> _logger;

    public ProposalAgentService(
        GreedyProposalService greedy,
        OrToolsProposalService orTools,
        IOptions<RosterOptions> options,
        ILogger<ProposalAgentService> logger)
    {
        _logger = logger;

        var algo = (options.Value.Algorithm ?? "Greedy").Trim();

        _strategy = algo.Equals("OrTools", StringComparison.OrdinalIgnoreCase)
            ? orTools
            : (IProposalGenerationStrategy)greedy;

        if (!algo.Equals("Greedy", StringComparison.OrdinalIgnoreCase) &&
            !algo.Equals("OrTools", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Unknown RosterGeneration:Algorithm value '{Value}' - falling back to Greedy", algo);
        }

        _logger.LogInformation("Roster generation algorithm selected: {Algorithm}", _strategy.GetType().Name);
    }

    public Task GenerateAsync(int proposalId, CancellationToken cancellationToken = default)
        => _strategy.GenerateAsync(proposalId, cancellationToken);
}
