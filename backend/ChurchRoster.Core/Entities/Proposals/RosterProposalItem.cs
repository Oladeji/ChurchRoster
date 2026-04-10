namespace ChurchRoster.Core.Entities.Proposals;

public class RosterProposalItem
{
    public int ItemId { get; set; }
    public int ProposalId { get; set; }
    public int TaskId { get; set; }
    public int UserId { get; set; }
    public DateOnly EventDate { get; set; }
    public ProposalItemStatus Status { get; set; } = ProposalItemStatus.Proposed;
    public string? SkipReason { get; set; }

    // Navigation properties
    public RosterProposal Proposal { get; set; } = null!;
    public MinistryTask Task { get; set; } = null!;
    public User User { get; set; } = null!;
}
