using ChurchRoster.Application.DTOs.Members;
using ChurchRoster.Application.Interfaces;
using ChurchRoster.Core.Entities;
using ChurchRoster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChurchRoster.Application.Services
{
    public class MemberService : IMemberService
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;

        public MemberService(AppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public async Task<IEnumerable<MemberDto>> GetAllMembersAsync()
        {
            var members = await _context.Users
                .Include(u => u.UserSkills)
                    .ThenInclude(us => us.Skill)
                .OrderBy(u => u.Name)
                .ToListAsync();

            return members.Select(MapToDto);
        }

        public async Task<MemberDto?> GetMemberByIdAsync(int userId)
        {
            var member = await _context.Users
                .Include(u => u.UserSkills)
                    .ThenInclude(us => us.Skill)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            return member == null ? null : MapToDto(member);
        }

        public async Task<MemberDto?> CreateMemberAsync(CreateMemberRequest request)
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return null;

            // Validate password if provided
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                if (!IsValidPassword(request.Password))
                    return null;
            }

            var member = new User
            {
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                PasswordHash = string.IsNullOrWhiteSpace(request.Password) 
                    ? _authService.HashPassword("TempPass@123") // Temporary password
                    : _authService.HashPassword(request.Password),
                Role = request.Role ?? "Member",
                MonthlyLimit = request.MonthlyLimit ?? 4,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(member);
            await _context.SaveChangesAsync();

            return await GetMemberByIdAsync(member.UserId);
        }

        public async Task<MemberDto?> UpdateMemberAsync(int userId, UpdateMemberRequest request)
        {
            var member = await _context.Users.FindAsync(userId);
            if (member == null)
                return null;

            member.Name = request.Name;
            member.Phone = request.Phone;
            member.Role = request.Role;
            member.MonthlyLimit = request.MonthlyLimit;
            member.IsActive = request.IsActive;
            member.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetMemberByIdAsync(userId);
        }

        public async Task<bool> DeleteMemberAsync(int userId)
        {
            var member = await _context.Users.FindAsync(userId);
            if (member == null)
                return false;

            // Soft delete - just deactivate
            member.IsActive = false;
            member.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePasswordAsync(int userId, UpdatePasswordRequest request)
        {
            var member = await _context.Users.FindAsync(userId);
            if (member == null)
                return false;

            // Verify current password
            if (!_authService.VerifyPassword(request.CurrentPassword, member.PasswordHash))
                return false;

            // Validate new password
            if (!IsValidPassword(request.NewPassword))
                return false;

            member.PasswordHash = _authService.HashPassword(request.NewPassword);
            member.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<MemberDto>> GetMembersByRoleAsync(string role)
        {
            var members = await _context.Users
                .Include(u => u.UserSkills)
                    .ThenInclude(us => us.Skill)
                .Where(u => u.Role == role)
                .OrderBy(u => u.Name)
                .ToListAsync();

            return members.Select(MapToDto);
        }

        public async Task<IEnumerable<MemberDto>> GetActiveMembersAsync()
        {
            var members = await _context.Users
                .Include(u => u.UserSkills)
                    .ThenInclude(us => us.Skill)
                .Where(u => u.IsActive)
                .OrderBy(u => u.Name)
                .ToListAsync();

            return members.Select(MapToDto);
        }

        private MemberDto MapToDto(User member)
        {
            return new MemberDto(
                member.UserId,
                member.Name,
                member.Email,
                member.Phone,
                member.Role,
                member.MonthlyLimit,
                member.IsActive,
                member.CreatedAt,
                member.UserSkills.Select(us => us.Skill.SkillName).ToList()
            );
        }

        private bool IsValidPassword(string password)
        {
            if (password.Length < 8)
                return false;

            bool hasUpperCase = password.Any(char.IsUpper);
            bool hasLowerCase = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);

            return hasUpperCase && hasLowerCase && hasDigit;
        }
    }
}
