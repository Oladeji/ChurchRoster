using System.Threading.Channels;
using ChurchRoster.Core.Entities.Proposals;
using ChurchRoster.Infrastructure.Data;
using ChurchRoster.Infrastructure.Services.Proposals;
using Microsoft.EntityFrameworkCore;

namespace ChurchRoster.Api.BackgroundServices;

/// <summary>
/// Reads proposal IDs from a bounded channel and runs the AI agent for each one.
/// Registered as a singleton-hosted BackgroundService; uses IServiceScopeFactory
/// to resolve scoped services (AppDbContext, ProposalAgentService, ProposalAgentTools)
/// per generation job — matching the pattern used by AssignmentStatusUpdateService.
/// </summary>
public class ProposalGenerationJob : BackgroundService
{
    private readonly Channel<int> _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ProposalGenerationJob> _logger;

    public ProposalGenerationJob(
        Channel<int> channel,
        IServiceScopeFactory scopeFactory,
        ILogger<ProposalGenerationJob> logger)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ProposalGenerationJob started — waiting for work");

        await foreach (var proposalId in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            _logger.LogInformation("ProposalGenerationJob dequeued ProposalId={ProposalId} — starting generation", proposalId);

            try
            {
                await RunGenerationAsync(proposalId, stoppingToken);
                _logger.LogInformation("ProposalGenerationJob completed ProposalId={ProposalId}", proposalId);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogWarning("ProposalGenerationJob stopping — ProposalId={ProposalId} was cancelled", proposalId);
                break;
            }
            catch (Exception ex)
            {
                // Log full exception so it is visible in the debug output window
                _logger.LogError(ex,
                    "ProposalGenerationJob FAILED for ProposalId={ProposalId} — {ExType}: {ExMsg}",
                    proposalId, ex.GetType().Name, ex.Message);
            }
        }

        _logger.LogInformation("ProposalGenerationJob stopped");
    }

    private async Task RunGenerationAsync(int proposalId, CancellationToken stoppingToken)
    {
        // Each generation gets its own DI scope so AppDbContext / EF tracking is isolated
        using var scope = _scopeFactory.CreateScope();
        var agentService = scope.ServiceProvider.GetRequiredService<ProposalAgentService>();
        await agentService.GenerateAsync(proposalId, stoppingToken);
    }
}
