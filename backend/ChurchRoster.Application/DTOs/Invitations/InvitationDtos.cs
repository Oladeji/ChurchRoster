namespace ChurchRoster.Application.DTOs.Invitations
{
    public record SendInvitationRequest(
        string Email,
        string Name,
        string Phone,
        string Role
    );

    public record InvitationDto(
        int InvitationId,
        string Email,
        string Name,
        string Phone,
        string Role,
        string Token,
        DateTime CreatedAt,
        DateTime ExpiresAt,
        bool IsUsed,
        DateTime? UsedAt,
        string CreatedByName
    );

    public record AcceptInvitationRequest(
        string Token,
        string Password
    );

    public record VerifyInvitationResponse(
        bool IsValid,
        string? Email,
        string? Name,
        string? Phone,
        string? Role,
        DateTime? ExpiresAt,
        string? Message
    );
}
