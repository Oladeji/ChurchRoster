namespace ChurchRoster.Core.Entities;

public class Assignment
{
    public int AssignmentId { get; set; }
    public int TenantId { get; set; }
    public int TaskId { get; set; }
    public int UserId { get; set; }
    public DateTime EventDate { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected, Confirmed, Completed, Expired
    public string? RejectionReason { get; set; }
    public bool IsOverride { get; set; } = false;
    public int AssignedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public MinistryTask Task { get; set; } = null!;
    public User User { get; set; } = null!;
    public User AssignedByUser { get; set; } = null!;
}
