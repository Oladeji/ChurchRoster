namespace ChurchRoster.Infrastructure.Services.Proposals;

public interface IProposalAgentTools
{
    Task<IEnumerable<AgentTaskDto>> GetRecurringTasksAsync(int tenantId);
    Task<IEnumerable<AgentMemberDto>> GetQualifiedMembersAsync(int tenantId, int taskId);
    Task<int> GetMemberAssignmentCountAsync(int userId, int month, int year);
    Task<IEnumerable<AgentAssignmentDto>> GetExistingAssignmentsAsync(int tenantId, DateOnly startDate, DateOnly endDate);
    Task<int> CreateProposalItemAsync(int proposalId, int taskId, int userId, DateOnly eventDate);
    Task LogSkippedSlotAsync(int proposalId, int taskId, DateOnly eventDate, string reason);
}
