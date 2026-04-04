using ChurchRoster.Application.DTOs.Invitations;
using ChurchRoster.Application.Interfaces;
using ChurchRoster.Core.Entities;
using ChurchRoster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace ChurchRoster.Application.Services
{
    public class InvitationService : IInvitationService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IAuthService _authService;
        private readonly ILogger<InvitationService> _logger;

        public InvitationService(AppDbContext context, IEmailService emailService, IAuthService authService, ILogger<InvitationService> logger)
        {
            _context = context;
            _emailService = emailService;
            _authService = authService;
            _logger = logger;
        }

        public async Task<InvitationDto?> SendInvitationAsync(SendInvitationRequest request, int createdByUserId)
        {
            _logger.LogInformation("=== SendInvitationAsync started ===");
            _logger.LogInformation("Email: {Email}, Name: {Name}, Role: {Role}, CreatedBy: {CreatedBy}", 
                request.Email, request.Name, request.Role, createdByUserId);

            // Check if user already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("❌ User already exists with email: {Email}", request.Email);
                return null; // User already exists
            }

            // Check if there's already a pending invitation
            var existingInvitation = await _context.Invitations
                .FirstOrDefaultAsync(i => i.Email == request.Email && !i.IsUsed && i.ExpiresAt > DateTime.UtcNow);

            if (existingInvitation != null)
            {
                _logger.LogWarning("❌ Pending invitation already exists for email: {Email}", request.Email);
                return null; // Pending invitation already exists
            }

            _logger.LogInformation("✅ No existing user or pending invitation found");

            // Generate unique token
            var token = GenerateSecureToken();
            _logger.LogInformation("Generated invitation token: {TokenPreview}...", token.Substring(0, Math.Min(10, token.Length)));

            var invitation = new Invitation
            {
                Email = request.Email,
                Name = request.Name,
                Phone = request.Phone,
                Role = request.Role,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsUsed = false,
                CreatedBy = createdByUserId
            };

            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Invitation saved to database with ID: {InvitationId}", invitation.InvitationId);

            // Get the admin who sent the invitation
            var admin = await _context.Users.FindAsync(createdByUserId);
            var adminName = admin?.Name ?? "Church Administrator";
            _logger.LogInformation("Admin name: {AdminName}", adminName);

            // Send invitation email
            _logger.LogInformation("📧 Attempting to send invitation email to {Email}...", request.Email);
            var emailSent = await _emailService.SendInvitationEmailAsync(request.Email, request.Name, token, adminName);

            if (emailSent)
            {
                _logger.LogInformation("✅✅ Invitation email sent successfully to {Email}", request.Email);
            }
            else
            {
                _logger.LogError("❌❌ FAILED to send invitation email to {Email}", request.Email);
                _logger.LogError("The invitation was saved to database but the email was NOT sent!");
                _logger.LogError("Check email service configuration and logs above for details.");
            }

            return await GetInvitationDtoAsync(invitation.InvitationId);
        }

        public async Task<VerifyInvitationResponse> VerifyInvitationTokenAsync(string token)
        {
            var invitation = await _context.Invitations
                .FirstOrDefaultAsync(i => i.Token == token);

            if (invitation == null)
            {
                return new VerifyInvitationResponse(false, null, null, null, null, null, "Invalid invitation token");
            }

            if (invitation.IsUsed)
            {
                return new VerifyInvitationResponse(false, null, null, null, null, null, "This invitation has already been used");
            }

            if (invitation.ExpiresAt < DateTime.UtcNow)
            {
                return new VerifyInvitationResponse(false, null, null, null, null, null, "This invitation has expired");
            }

            return new VerifyInvitationResponse(true, invitation.Email, invitation.Name, invitation.Phone, invitation.Role, invitation.ExpiresAt, "Invitation is valid");
        }

        public async Task<bool> AcceptInvitationAsync(AcceptInvitationRequest request)
        {
            var invitation = await _context.Invitations
                .FirstOrDefaultAsync(i => i.Token == request.Token);

            if (invitation == null || invitation.IsUsed || invitation.ExpiresAt < DateTime.UtcNow)
            {
                return false;
            }

            // Check if user already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == invitation.Email);
            if (existingUser != null)
            {
                return false; // User already exists
            }

            // Create the user account using AuthService
            var registerRequest = new DTOs.Auth.RegisterRequest(
                invitation.Name,
                invitation.Email,
                request.Password,
                invitation.Phone
            );

            var authResponse = await _authService.RegisterAsync(registerRequest);

            if (authResponse == null)
            {
                return false;
            }

            // Update the user's role to match the invitation
            var user = await _context.Users.FindAsync(authResponse.UserId);
            if (user != null)
            {
                user.Role = invitation.Role;
            }

            // Mark invitation as used
            invitation.IsUsed = true;
            invitation.UsedAt = DateTime.UtcNow;
            invitation.UserId = authResponse.UserId;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<InvitationDto>> GetAllInvitationsAsync()
        {
            var invitations = await _context.Invitations
                .Include(i => i.CreatedByUser)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return invitations.Select(MapToDto);
        }

        public async Task<IEnumerable<InvitationDto>> GetPendingInvitationsAsync()
        {
            var invitations = await _context.Invitations
                .Include(i => i.CreatedByUser)
                .Where(i => !i.IsUsed && i.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return invitations.Select(MapToDto);
        }

        public async Task<bool> CancelInvitationAsync(int invitationId)
        {
            var invitation = await _context.Invitations.FindAsync(invitationId);
            if (invitation == null || invitation.IsUsed)
            {
                return false;
            }

            _context.Invitations.Remove(invitation);
            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<InvitationDto?> GetInvitationDtoAsync(int invitationId)
        {
            var invitation = await _context.Invitations
                .Include(i => i.CreatedByUser)
                .FirstOrDefaultAsync(i => i.InvitationId == invitationId);

            return invitation == null ? null : MapToDto(invitation);
        }

        private InvitationDto MapToDto(Invitation invitation)
        {
            return new InvitationDto(
                invitation.InvitationId,
                invitation.Email,
                invitation.Name,
                invitation.Phone,
                invitation.Role,
                invitation.Token,
                invitation.CreatedAt,
                invitation.ExpiresAt,
                invitation.IsUsed,
                invitation.UsedAt,
                invitation.CreatedByUser?.Name ?? "Admin"
            );
        }

        private string GenerateSecureToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }
    }
}
