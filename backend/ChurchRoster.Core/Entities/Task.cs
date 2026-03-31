namespace ChurchRoster.Core.Entities;

public class MinistryTask
{
    public int TaskId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty; // Weekly or Monthly
    public string DayRule { get; set; } = string.Empty; // Tuesday, Sunday, Last Friday, etc.
    public int? RequiredSkillId { get; set; }
    public bool IsRestricted { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Skill? RequiredSkill { get; set; }
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}
