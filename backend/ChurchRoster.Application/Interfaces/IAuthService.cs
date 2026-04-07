using ChurchRoster.Application.DTOs.Auth;

namespace ChurchRoster.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse?> LoginAsync(LoginRequest request);
        Task<AuthResponse?> RegisterAsync(RegisterRequest request);
        string GenerateJwtToken(int userId, int tenantId, string email, string role);
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }
}
