namespace ChurchRoster.Application.DTOs.Skills
{
    public record CreateSkillRequest(
        string SkillName,
        string? Description
    );
}
