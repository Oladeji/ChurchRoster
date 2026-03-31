namespace ChurchRoster.Application.DTOs.Assignments
{
    public record AssignmentValidationResult(
        bool IsValid,
        List<string> Errors,
        List<string> Warnings
    );
}
