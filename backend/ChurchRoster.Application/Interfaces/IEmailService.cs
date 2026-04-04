namespace ChurchRoster.Application.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendInvitationEmailAsync(string toEmail, string toName, string invitationToken, string invitedByName);
        Task<bool> SendAssignmentNotificationAsync(string toEmail, string toName, string taskName, DateTime eventDate);
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string toName, string resetToken);
    }
}
