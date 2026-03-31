namespace ChurchRoster.Core.Entities;

public class User
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Member"; // Admin or Member
    public int? MonthlyLimit { get; set; }
    public string? DeviceToken { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<Assignment> AssignmentsCreated { get; set; } = new List<Assignment>();
}
