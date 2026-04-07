using ChurchRoster.Application.DTOs.Skills;
using ChurchRoster.Application.Interfaces;
using ChurchRoster.Core.Entities;
using ChurchRoster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChurchRoster.Application.Services
{
    public class SkillService : ISkillService
    {
        private readonly AppDbContext _context;
        private readonly ITenantContext _tenantContext;

        public SkillService(AppDbContext context, ITenantContext tenantContext)
        {
            _context = context;
            _tenantContext = tenantContext;
        }

        public async Task<IEnumerable<SkillDto>> GetAllSkillsAsync()
        {
            var skills = await _context.Skills
                .OrderBy(s => s.SkillName)
                .ToListAsync();

            return skills.Select(MapToDto);
        }

        public async Task<SkillDto?> GetSkillByIdAsync(int skillId)
        {
            var skill = await _context.Skills.FirstOrDefaultAsync(s => s.SkillId == skillId);
            return skill == null ? null : MapToDto(skill);
        }

        public async Task<SkillDto?> CreateSkillAsync(CreateSkillRequest request)
        {
            if (!_tenantContext.TenantId.HasValue)
                return null;

            // Check if skill name already exists
            if (await _context.Skills.AnyAsync(s => s.SkillName == request.SkillName))
                return null;

            var skill = new Skill
            {
                TenantId = _tenantContext.TenantId.Value,
                SkillName = request.SkillName,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Skills.Add(skill);
            await _context.SaveChangesAsync();

            return MapToDto(skill);
        }

        public async Task<SkillDto?> UpdateSkillAsync(int skillId, UpdateSkillRequest request)
        {
            var skill = await _context.Skills.FirstOrDefaultAsync(s => s.SkillId == skillId);
            if (skill == null)
                return null;

            // Check if new name conflicts with existing skill
            if (await _context.Skills.AnyAsync(s => s.SkillName == request.SkillName && s.SkillId != skillId))
                return null;

            skill.SkillName = request.SkillName;
            skill.Description = request.Description;
            skill.IsActive = request.IsActive;
            skill.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToDto(skill);
        }

        public async Task<bool> DeleteSkillAsync(int skillId)
        {
            var skill = await _context.Skills.FirstOrDefaultAsync(s => s.SkillId == skillId);
            if (skill == null)
                return false;

            // Soft delete - deactivate
            skill.IsActive = false;
            skill.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserSkillDto?> AssignSkillToUserAsync(AssignSkillRequest request)
        {
            // Check if user exists
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == request.UserId);
            if (user == null)
                return null;

            // Check if skill exists
            var skill = await _context.Skills.FirstOrDefaultAsync(s => s.SkillId == request.SkillId);
            if (skill == null)
                return null;

            if (user.TenantId != skill.TenantId)
                return null;

            // Check if already assigned
            if (await _context.UserSkills.AnyAsync(us => us.UserId == request.UserId && us.SkillId == request.SkillId))
                return null;

            var userSkill = new UserSkill
            {
                TenantId = user.TenantId,
                UserId = request.UserId,
                SkillId = request.SkillId,
                AssignedDate = DateTime.UtcNow
            };

            _context.UserSkills.Add(userSkill);
            await _context.SaveChangesAsync();

            return new UserSkillDto(
                user.UserId,
                user.Name,
                skill.SkillId,
                skill.SkillName,
                userSkill.AssignedDate
            );
        }

        public async Task<bool> RemoveSkillFromUserAsync(int userId, int skillId)
        {
            var userSkill = await _context.UserSkills
                .FirstOrDefaultAsync(us => us.UserId == userId && us.SkillId == skillId);

            if (userSkill == null)
                return false;

            _context.UserSkills.Remove(userSkill);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<UserSkillDto>> GetUserSkillsAsync(int userId)
        {
            var userSkills = await _context.UserSkills
                .Include(us => us.User)
                .Include(us => us.Skill)
                .Where(us => us.UserId == userId)
                .ToListAsync();

            return userSkills.Select(us => new UserSkillDto(
                us.User.UserId,
                us.User.Name,
                us.Skill.SkillId,
                us.Skill.SkillName,
                us.AssignedDate
            ));
        }

        public async Task<IEnumerable<UserSkillDto>> GetSkillUsersAsync(int skillId)
        {
            var userSkills = await _context.UserSkills
                .Include(us => us.User)
                .Include(us => us.Skill)
                .Where(us => us.SkillId == skillId)
                .ToListAsync();

            return userSkills.Select(us => new UserSkillDto(
                us.User.UserId,
                us.User.Name,
                us.Skill.SkillId,
                us.Skill.SkillName,
                us.AssignedDate
            ));
        }

        private SkillDto MapToDto(Skill skill)
        {
            return new SkillDto(
                skill.SkillId,
                skill.SkillName,
                skill.Description,
                skill.IsActive,
                skill.CreatedAt
            );
        }
    }
}
