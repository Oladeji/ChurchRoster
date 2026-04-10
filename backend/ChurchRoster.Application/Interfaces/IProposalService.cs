using ChurchRoster.Application.DTOs.Proposals;

namespace ChurchRoster.Application.Interfaces;

public interface IProposalService
{
    // Commands
    Task<GenerateProposalResult?> GenerateProposalAsync(GenerateProposalRequest request, int createdByUserId);
    Task<ProposalItemDto?> UpdateProposalItemAsync(int itemId, UpdateProposalItemRequest request);
    Task<ProposalItemDto?> AddProposalItemAsync(int proposalId, AddProposalItemRequest request);
    Task<bool> DeleteProposalItemAsync(int itemId);
    Task<PublishProposalResult?> PublishProposalAsync(int proposalId);
    Task<bool> ArchiveProposalAsync(int proposalId);

    // Queries
    Task<ProposalDetailDto?> GetProposalByIdAsync(int proposalId);
    Task<IEnumerable<ProposalSummaryDto>> GetProposalListAsync();
}
