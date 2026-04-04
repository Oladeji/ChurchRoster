using ChurchRoster.Application.DTOs.Members;
using ChurchRoster.Application.DTOs.Skills;

namespace ChurchRoster.Application.Interfaces
{
    public interface IMemberService
    {
        Task<IEnumerable<MemberDto>> GetAllMembersAsync();
        Task<MemberDto?> GetMemberByIdAsync(int userId);
        Task<MemberDto?> CreateMemberAsync(CreateMemberRequest request);
        Task<MemberDto?> UpdateMemberAsync(int userId, UpdateMemberRequest request);
        Task<bool> DeleteMemberAsync(int userId);
        Task<bool> UpdatePasswordAsync(int userId, UpdatePasswordRequest request);
        Task<IEnumerable<MemberDto>> GetMembersByRoleAsync(string role);
        Task<IEnumerable<MemberDto>> GetActiveMembersAsync();
        Task<IEnumerable<SkillDto>> GetMemberSkillsAsync(int userId);
        Task<bool> AssignSkillToMemberAsync(int userId, int skillId);
        Task<bool> RemoveSkillFromMemberAsync(int userId, int skillId);
        Task<IEnumerable<MemberDto>> GetQualifiedMembersForTaskAsync(int taskId);
        Task<bool> UpdateDeviceTokenAsync(int userId, string deviceToken);
    }
}
