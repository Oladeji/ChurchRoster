namespace ChurchRoster.Application.DTOs.Skills
{
    public record UpdateSkillRequest(
        string SkillName,
        string? Description,
        bool IsActive
    );
}
