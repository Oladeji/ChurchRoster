namespace ChurchRoster.Application.DTOs.Auth
{
    public record AuthResponse(
        int UserId,
        int TenantId,
        string Name,
        string Email,
        string Role,
        string Token,
        DateTime ExpiresAt
    );
}
