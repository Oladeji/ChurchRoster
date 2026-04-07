namespace ChurchRoster.Application.DTOs.Auth
{
    public record LoginRequest(
        int TenantId,
        string Email,
        string Password
    );
}
