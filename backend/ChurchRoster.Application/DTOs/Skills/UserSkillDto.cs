namespace ChurchRoster.Application.DTOs.Skills
{
    public record UserSkillDto(
        int UserId,
        string UserName,
        int SkillId,
        string SkillName,
        DateTime AssignedDate
    );
}
