using ChurchRoster.Application.DTOs.Invitations;

namespace ChurchRoster.Application.Interfaces
{
    public interface IInvitationService
    {
        Task<InvitationDto?> SendInvitationAsync(SendInvitationRequest request, int createdByUserId);
        Task<VerifyInvitationResponse> VerifyInvitationTokenAsync(string token);
        Task<bool> AcceptInvitationAsync(AcceptInvitationRequest request);
        Task<IEnumerable<InvitationDto>> GetAllInvitationsAsync();
        Task<IEnumerable<InvitationDto>> GetPendingInvitationsAsync();
        Task<bool> CancelInvitationAsync(int invitationId);
    }
}
