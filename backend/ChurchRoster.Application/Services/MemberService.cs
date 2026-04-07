using ChurchRoster.Application.DTOs.Members;
using ChurchRoster.Application.DTOs.Skills;
using ChurchRoster.Application.Interfaces;
using ChurchRoster.Core.Entities;
using ChurchRoster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChurchRoster.Application.Services
{
    public class MemberService : IMemberService
    {
        private readonly AppDbContext _context;
        private readonly ITenantContext _tenantContext;
        private readonly IAuthService _authService;

        public MemberService(AppDbContext context, ITenantContext tenantContext, IAuthService authService)
        {
            _context = context;
            _tenantContext = tenantContext;
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
            if (!_tenantContext.TenantId.HasValue)
                return null;

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
                TenantId = _tenantContext.TenantId.Value,
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
            var member = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
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
            var member = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
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
            var member = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
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

        public async Task<IEnumerable<SkillDto>> GetMemberSkillsAsync(int userId)
        {
            var member = await _context.Users
                .Include(u => u.UserSkills)
                    .ThenInclude(us => us.Skill)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (member == null)
                return Enumerable.Empty<SkillDto>();

            return member.UserSkills
                .Select(us => new SkillDto(
                    us.Skill.SkillId,
                    us.Skill.SkillName,
                    us.Skill.Description,
                    us.Skill.IsActive,
                    us.Skill.CreatedAt
                ))
                .ToList();
        }

        public async Task<bool> AssignSkillToMemberAsync(int userId, int skillId)
        {
            var member = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (member == null)
                return false;

            // Check if skill exists
            var skill = await _context.Skills
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SkillId == skillId);

            if (skill == null)
                return false;

            if (member.TenantId != skill.TenantId)
                return false;

            // Check if already assigned
            var alreadyAssigned = await _context.UserSkills
                .AnyAsync(us => us.UserId == userId && us.SkillId == skillId);

            if (alreadyAssigned)
                return false;

            // Create assignment
            var userSkill = new UserSkill
            {
                TenantId = member.TenantId,
                UserId = userId,
                SkillId = skillId,
                AssignedDate = DateTime.UtcNow
            };

            _context.UserSkills.Add(userSkill);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveSkillFromMemberAsync(int userId, int skillId)
        {
            var userSkill = await _context.UserSkills
                .FirstOrDefaultAsync(us => us.UserId == userId && us.SkillId == skillId);

            if (userSkill == null)
                return false;

            _context.UserSkills.Remove(userSkill);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<MemberDto>> GetQualifiedMembersForTaskAsync(int taskId)
        {
            // Get the task and its required skill
            var task = await _context.Tasks
                .Include(t => t.RequiredSkill)
                .FirstOrDefaultAsync(t => t.TaskId == taskId);

            if (task == null)
                return Enumerable.Empty<MemberDto>();

            // If no skill required, return all active members
            if (task.RequiredSkillId == null)
            {
                return await GetActiveMembersAsync();
            }

            // Get members with the required skill
            var qualifiedMembers = await _context.Users
                .Include(u => u.UserSkills)
                    .ThenInclude(us => us.Skill)
                .Where(u => u.IsActive && 
                           u.UserSkills.Any(us => us.SkillId == task.RequiredSkillId))
                .OrderBy(u => u.Name)
                .ToListAsync();

            return qualifiedMembers.Select(MapToDto);
        }

        public async Task<bool> UpdateDeviceTokenAsync(int userId, string deviceToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return false;

            user.DeviceToken = deviceToken;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
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
                member.UserSkills.Select(us => us.Skill.SkillName).ToList(),
                member.DeviceToken
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
