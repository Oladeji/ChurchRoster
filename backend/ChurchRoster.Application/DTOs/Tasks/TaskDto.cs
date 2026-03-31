namespace ChurchRoster.Application.DTOs.Tasks
{
    public record TaskDto(
        int TaskId,
        string TaskName,
        string Frequency,
        string DayRule,
        int? RequiredSkillId,
        string? RequiredSkillName,
        bool IsRestricted,
        bool IsActive,
        DateTime CreatedAt
    );
}
