namespace ChurchRoster.Application.DTOs.Members
{
    public record MemberDto(
        int UserId,
        string Name,
        string Email,
        string? Phone,
        string Role,
        int? MonthlyLimit,
        bool IsActive,
        DateTime CreatedAt,
        List<string> Skills
    );
}
