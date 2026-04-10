using System.ComponentModel;
using ChurchRoster.Core.Entities.Proposals;
using ChurchRoster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChurchRoster.Infrastructure.Services.Proposals;

/// <summary>
/// Concrete implementation of the agent tool functions.
/// Each method is annotated with [Description] so Microsoft.Extensions.AI
/// can auto-generate the tool schema exposed to the LLM.
/// </summary>
public class ProposalAgentTools : IProposalAgentTools
{
    private readonly AppDbContext _db;

    public ProposalAgentTools(AppDbContext db)
    {
        _db = db;
    }

    [Description("Returns all active recurring tasks for the given tenant, including their day rules and required skill IDs.")]
    public async Task<IEnumerable<AgentTaskDto>> GetRecurringTasksAsync(
        [Description("The tenant ID to fetch tasks for.")] int tenantId)
    {
        return await _db.Tasks
            .Where(t => t.TenantId == tenantId && t.IsActive)
            .Select(t => new AgentTaskDto(
                t.TaskId,
                t.TaskName,
                t.Frequency,
                t.DayRule,
                t.RequiredSkillId,
                t.IsRestricted))
            .ToListAsync();
    }

    [Description("Returns all active members in the tenant who are qualified to perform a given task. For unrestricted tasks pass taskId = 0 to get all active members.")]
    public async Task<IEnumerable<AgentMemberDto>> GetQualifiedMembersAsync(
        [Description("The tenant ID.")] int tenantId,
        [Description("The task ID. Pass 0 to retrieve all active members regardless of skill.")] int taskId)
    {
        // Find whether the task requires a skill
        int? requiredSkillId = taskId == 0
            ? null
            : await _db.Tasks
                .Where(t => t.TenantId == tenantId && t.TaskId == taskId)
                .Select(t => t.RequiredSkillId)
                .FirstOrDefaultAsync();

        var query = _db.Users
            .Where(u => u.TenantId == tenantId && u.IsActive && u.Role == "Member");

        if (requiredSkillId.HasValue)
        {
            query = query.Where(u =>
                u.UserSkills.Any(us => us.SkillId == requiredSkillId.Value));
        }

        return await query
            .Select(u => new AgentMemberDto(u.UserId, u.MonthlyLimit))
            .ToListAsync();
    }

    [Description("Returns the number of assignments already created (live) for a member in a specific month and year. Use this to respect monthly limits and distribute tasks fairly.")]
    public async Task<int> GetMemberAssignmentCountAsync(
        [Description("The user ID of the member.")] int userId,
        [Description("The month number (1–12).")] int month,
        [Description("The four-digit year.")] int year)
    {
        return await _db.Assignments
            .IgnoreQueryFilters()
            .CountAsync(a =>
                a.UserId == userId &&
                a.EventDate.Month == month &&
                a.EventDate.Year == year &&
                a.Status != "Expired" &&
                a.Status != "Rejected");
    }

    [Description("Returns all existing live assignments within a date range for the tenant. Use this to detect conflicts before proposing a slot.")]
    public async Task<IEnumerable<AgentAssignmentDto>> GetExistingAssignmentsAsync(
        [Description("The tenant ID.")] int tenantId,
        [Description("Start of the date range (inclusive), formatted as YYYY-MM-DD.")] DateOnly startDate,
        [Description("End of the date range (inclusive), formatted as YYYY-MM-DD.")] DateOnly endDate)
    {
        var start = startDate.ToDateTime(TimeOnly.MinValue);
        var end = endDate.ToDateTime(TimeOnly.MaxValue);

        return await _db.Assignments
            .IgnoreQueryFilters()
            .Where(a =>
                a.TenantId == tenantId &&
                a.EventDate >= start &&
                a.EventDate <= end &&
                a.Status != "Expired" &&
                a.Status != "Rejected")
            .Select(a => new AgentAssignmentDto(
                a.TaskId,
                a.UserId,
                DateOnly.FromDateTime(a.EventDate)))
            .ToListAsync();
    }

    [Description("Creates a single proposed assignment slot inside the draft proposal. Call this once per task-date-member combination you want to include in the roster.")]
    public async Task<int> CreateProposalItemAsync(
        [Description("The ID of the proposal being generated.")] int proposalId,
        [Description("The task ID to assign.")] int taskId,
        [Description("The user ID of the member being assigned.")] int userId,
        [Description("The date for this assignment, formatted as YYYY-MM-DD.")] DateOnly eventDate)
    {
        var item = new RosterProposalItem
        {
            ProposalId = proposalId,
            TaskId = taskId,
            UserId = userId,
            EventDate = eventDate,
            Status = ProposalItemStatus.Proposed
        };

        _db.RosterProposalItems.Add(item);
        await _db.SaveChangesAsync();
        return item.ItemId;
    }

    [Description("Logs a slot the agent could not fill, for example because no qualified member is available or the date falls outside business rules. This creates an audit record visible to the admin.")]
    public async Task LogSkippedSlotAsync(
        [Description("The ID of the proposal being generated.")] int proposalId,
        [Description("The task ID that could not be filled.")] int taskId,
        [Description("The date that could not be filled, formatted as YYYY-MM-DD.")] DateOnly eventDate,
        [Description("Human-readable reason why this slot was skipped.")] string reason)
    {
        var log = new ProposalSkipLog
        {
            ProposalId = proposalId,
            TaskId = taskId,
            EventDate = eventDate,
            Reason = reason,
            LoggedAt = DateTime.UtcNow
        };

        _db.ProposalSkipLogs.Add(log);
        await _db.SaveChangesAsync();
    }
}
