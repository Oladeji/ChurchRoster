namespace ChurchRoster.Application.DTOs.Members
{
    public record UpdateMemberRequest(
        string Name,
        string? Phone,
        string Role,
        int? MonthlyLimit,
        bool IsActive
    );
}
