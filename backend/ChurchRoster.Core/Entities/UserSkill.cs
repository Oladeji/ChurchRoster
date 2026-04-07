namespace ChurchRoster.Core.Entities;

public class UserSkill
{
    public int TenantId { get; set; }
    public int UserId { get; set; }
    public int SkillId { get; set; }
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public User User { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
