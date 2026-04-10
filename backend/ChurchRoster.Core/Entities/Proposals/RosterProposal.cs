namespace ChurchRoster.Core.Entities.Proposals;

public class RosterProposal
{
    public int ProposalId { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProposalStatus Status { get; set; } = ProposalStatus.Processing;
    public DateOnly DateRangeStart { get; set; }
    public DateOnly DateRangeEnd { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
    public int CreatedByUserId { get; set; }

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public ICollection<RosterProposalItem> Items { get; set; } = new List<RosterProposalItem>();
    public ICollection<ProposalSkipLog> SkipLogs { get; set; } = new List<ProposalSkipLog>();
}
