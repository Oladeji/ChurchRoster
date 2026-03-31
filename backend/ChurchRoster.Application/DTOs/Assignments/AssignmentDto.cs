namespace ChurchRoster.Application.DTOs.Assignments
{
    public record AssignmentDto(
        int AssignmentId,
        int TaskId,
        string TaskName,
        int UserId,
        string UserName,
        DateTime EventDate,
        string Status,
        string? RejectionReason,
        bool IsOverride,
        int AssignedBy,
        string AssignedByName,
        DateTime CreatedAt
    );
}
