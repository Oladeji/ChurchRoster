using ChurchRoster.Application.DTOs.Skills;

namespace ChurchRoster.Application.Interfaces
{
    public interface ISkillService
    {
        Task<IEnumerable<SkillDto>> GetAllSkillsAsync();
        Task<SkillDto?> GetSkillByIdAsync(int skillId);
        Task<SkillDto?> CreateSkillAsync(CreateSkillRequest request);
        Task<SkillDto?> UpdateSkillAsync(int skillId, UpdateSkillRequest request);
        Task<bool> DeleteSkillAsync(int skillId);
        Task<UserSkillDto?> AssignSkillToUserAsync(AssignSkillRequest request);
        Task<bool> RemoveSkillFromUserAsync(int userId, int skillId);
        Task<IEnumerable<UserSkillDto>> GetUserSkillsAsync(int userId);
        Task<IEnumerable<UserSkillDto>> GetSkillUsersAsync(int skillId);
    }
}
