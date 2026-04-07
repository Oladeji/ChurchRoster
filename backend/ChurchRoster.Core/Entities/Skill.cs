namespace ChurchRoster.Core.Entities;

public class Skill
{
    public int SkillId { get; set; }
    public int TenantId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
    public ICollection<MinistryTask> Tasks { get; set; } = new List<MinistryTask>();
}
