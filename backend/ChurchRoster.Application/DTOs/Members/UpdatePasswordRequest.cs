namespace ChurchRoster.Application.DTOs.Members
{
    public record UpdatePasswordRequest(
        string CurrentPassword,
        string NewPassword
    );
}
