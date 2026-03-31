namespace ChurchRoster.Application.DTOs.Tasks
{
    public record UpdateTaskRequest(
        string TaskName,
        string Frequency,
        string DayRule,
        int? RequiredSkillId,
        bool IsRestricted,
        bool IsActive
    );
}
