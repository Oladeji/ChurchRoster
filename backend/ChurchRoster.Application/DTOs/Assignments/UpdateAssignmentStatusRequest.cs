namespace ChurchRoster.Application.DTOs.Assignments
{
    public record UpdateAssignmentStatusRequest(
        string Status,
        string? RejectionReason
    );
}
