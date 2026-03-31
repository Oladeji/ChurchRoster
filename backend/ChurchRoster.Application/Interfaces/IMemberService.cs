using ChurchRoster.Application.DTOs.Members;

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
    }
}
