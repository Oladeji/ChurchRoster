namespace ChurchRoster.Application.DTOs.Auth
{
    public record RegisterRequest(
        int TenantId,
        string Name,
        string Email,
        string Password,
        string Phone
    );
}
