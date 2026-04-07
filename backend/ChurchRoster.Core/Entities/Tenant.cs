namespace ChurchRoster.Core.Entities;

public class Tenant
{
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string ContactEmail { get; set; } = string.Empty;
    public DateTime SubscriptionEndDate { get; set; }
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Skill> Skills { get; set; } = new List<Skill>();
    public ICollection<MinistryTask> Tasks { get; set; } = new List<MinistryTask>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();
}
