namespace ChurchRoster.Application.DTOs.Skills
{
    public record SkillDto(
        int SkillId,
        string SkillName,
        string? Description,
        bool IsActive,
        DateTime CreatedAt
    );
}
