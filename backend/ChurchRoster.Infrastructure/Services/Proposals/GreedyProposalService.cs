using ChurchRoster.Core.Entities.Proposals;
using ChurchRoster.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace ChurchRoster.Infrastructure.Services.Proposals;

/// <summary>
/// Greedy roster generation algorithm.
///
/// Processes each scheduled slot in order and applies the same rules as the
/// former LLM-based service — but entirely in deterministic C# code:
///
///   1. Skip if a live assignment already covers this task+date.
///   2. Filter to skill-qualified members.
///   3. Remove members already assigned on the same date (any task).
///   4. Remove members who have reached their monthly limit
///      (unless ALL remaining candidates are at the limit — then keep all).
///   5. Pick the member with the lowest monthly assignment count;
///      tie-break by lowest UserId.
///
/// All in-progress assignments are tracked in memory so each subsequent slot
/// sees the accumulated state of earlier assignments in the same run.
/// </summary>
public class GreedyProposalService : ProposalGenerationBase
{
    public GreedyProposalService(
        AppDbContext db,
        ITenantContext tenantContext,
        ILogger<GreedyProposalService> logger)
        : base(db, tenantContext, logger) { }

    protected override string AlgorithmName => "Greedy";

    protected override Task RunAlgorithmAsync(
        RosterProposal proposal,
        ProposalGenerationContext ctx,
        CancellationToken cancellationToken)
    {
        var proposalId = proposal.ProposalId;

        // ── Working state ─────────────────────────────────────────────────────

        // Existing live assignments: (taskId, date) → already covered
        var existingByTaskDate = new HashSet<(int, string)>(
            ctx.ExistingAssignments.Select(a => (a.TaskId, a.EventDate)));

        // Existing same-day members: date → set of userIds already busy
        var busyOnDate = ctx.ExistingAssignments
            .GroupBy(a => a.EventDate)
            .ToDictionary(g => g.Key, g => new HashSet<int>(g.Select(a => a.UserId)));

        // In-run monthly counts per member: (userId, year, month) → count
        var runCounts = ctx.AssignmentCounts
            .ToDictionary(
                c => (c.UserId, c.Year, c.Month),
                c => c.Count);

        // ── Process each slot ─────────────────────────────────────────────────
        foreach (var slot in ctx.ScheduledSlots)
        {
            var date = slot.EventDate; // "yyyy-MM-dd"

            // Step 1 — existing conflict
            if (existingByTaskDate.Contains((slot.TaskId, date)))
            {
                AddSkipLog(proposalId, slot, "Live assignment already exists for this task on this date");
                continue;
            }

            // Step 2 — skill filter
            var eligible = slot.RequiredSkillId is null or 0
                ? ctx.Members.ToList()
                : ctx.Members.Where(m => m.SkillIds.Contains(slot.RequiredSkillId.Value)).ToList();

            if (eligible.Count == 0)
            {
                AddSkipLog(proposalId, slot, $"No member holds requiredSkill");
                continue;
            }

            // Step 3 — same-day exclusion
            if (busyOnDate.TryGetValue(date, out var busyIds))
                eligible = eligible.Where(m => !busyIds.Contains(m.UserId)).ToList();

            if (eligible.Count == 0)
            {
                AddSkipLog(proposalId, slot, "All eligible members are already assigned to another task on this date");
                continue;
            }

            // Step 4 — monthly limit (parse month/year once)
            var parsed = DateOnly.Parse(date);
            var (yr, mo) = (parsed.Year, parsed.Month);

            int GetTotal(int userId) =>
                runCounts.TryGetValue((userId, yr, mo), out var c) ? c : 0;

            var underLimit = eligible
                .Where(m => m.MonthlyLimit is null || GetTotal(m.UserId) < m.MonthlyLimit)
                .ToList();

            // Only apply limit exclusion if at least one candidate is under limit;
            // otherwise keep all (fair overflow)
            if (underLimit.Count > 0)
                eligible = underLimit;

            if (eligible.Count == 0)
            {
                AddSkipLog(proposalId, slot, "Monthly limit reached for all eligible members");
                continue;
            }

            // Step 5 — pick member with lowest count; tie-break by lowest userId
            var chosen = eligible
                .OrderBy(m => GetTotal(m.UserId))
                .ThenBy(m => m.UserId)
                .First();

            // Record assignment
            var eventDate = DateOnly.Parse(date);
            Db.RosterProposalItems.Add(new RosterProposalItem
            {
                ProposalId = proposalId,
                TaskId     = slot.TaskId,
                UserId     = chosen.UserId,
                EventDate  = eventDate,
                Status     = ProposalItemStatus.Proposed,
            });

            // Update in-run state
            var key = (chosen.UserId, yr, mo);
            runCounts[key] = GetTotal(chosen.UserId) + 1;

            if (!busyOnDate.TryGetValue(date, out var set))
            {
                set = [];
                busyOnDate[date] = set;
            }
            set.Add(chosen.UserId);
        }

        Logger.LogInformation("[Greedy] ProposalId={ProposalId} — slot processing complete", proposalId);
        return Task.CompletedTask;
    }

    private void AddSkipLog(int proposalId, AgentScheduledSlotDto slot, string reason)
    {
        Logger.LogInformation("[Greedy] Skipping TaskId={TaskId} Date={Date} — {Reason}",
            slot.TaskId, slot.EventDate, reason);

        Db.ProposalSkipLogs.Add(new ProposalSkipLog
        {
            ProposalId = proposalId,
            TaskId     = slot.TaskId,
            EventDate  = DateOnly.Parse(slot.EventDate),
            Reason     = reason,
            LoggedAt   = DateTime.UtcNow,
        });
    }
}
