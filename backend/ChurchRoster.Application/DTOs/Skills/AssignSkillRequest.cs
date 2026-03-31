namespace ChurchRoster.Application.DTOs.Skills
{
    public record AssignSkillRequest(
        int UserId,
        int SkillId
    );
}
