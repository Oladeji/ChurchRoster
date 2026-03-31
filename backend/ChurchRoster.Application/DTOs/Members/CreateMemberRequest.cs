namespace ChurchRoster.Application.DTOs.Members
{
    public record CreateMemberRequest(
        string Name,
        string Email,
        string Password,
        string? Phone,
        string Role,
        int? MonthlyLimit
    );
}
