namespace ChurchRoster.Application.DTOs.Assignments
{
    public record CreateAssignmentRequest(
        int TaskId,
        int UserId,
        DateTime EventDate,
        bool IsOverride
    );
}
