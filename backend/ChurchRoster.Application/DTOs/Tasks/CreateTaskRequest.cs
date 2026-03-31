namespace ChurchRoster.Application.DTOs.Tasks
{
    public record CreateTaskRequest(
        string TaskName,
        string Frequency,
        string DayRule,
        int? RequiredSkillId,
        bool IsRestricted
    );
}
