namespace ChurchRoster.Core.Entities.Proposals;

public class ProposalSkipLog
{
    public int LogId { get; set; }
    public int ProposalId { get; set; }
    public int TaskId { get; set; }
    public DateOnly EventDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public RosterProposal Proposal { get; set; } = null!;
    public MinistryTask Task { get; set; } = null!;
}
