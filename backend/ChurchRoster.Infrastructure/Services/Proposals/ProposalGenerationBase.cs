using ChurchRoster.Core.Entities.Proposals;
using ChurchRoster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChurchRoster.Infrastructure.Services.Proposals;

/// <summary>
/// Shared data-fetching and slot-computation logic used by both
/// <see cref="GreedyProposalService"/> and <see cref="OrToolsProposalService"/>.
/// </summary>
public abstract class ProposalGenerationBase : IProposalGenerationStrategy
{
    protected readonly AppDbContext Db;
    protected readonly ITenantContext TenantContext;
    protected readonly ILogger Logger;

    protected ProposalGenerationBase(
        AppDbContext db,
        ITenantContext tenantContext,
        ILogger logger)
    {
        Db = db;
        TenantContext = tenantContext;
        Logger = logger;
    }

    // ── Public entry point ────────────────────────────────────────────────────

    public async Task GenerateAsync(int proposalId, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("[{Algo}] Generation starting for ProposalId={ProposalId}",
            AlgorithmName, proposalId);

        var proposal = await Db.RosterProposals
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.ProposalId == proposalId, cancellationToken);

        if (proposal is null)
        {
            Logger.LogError("ProposalId={ProposalId} not found — aborting", proposalId);
            return;
        }

        try
        {
            TenantContext.TenantId = proposal.TenantId;

            var context = await FetchContextAsync(proposal, cancellationToken);

            await RunAlgorithmAsync(proposal, context, cancellationToken);

            proposal.Status = ProposalStatus.Draft;
            await Db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("[{Algo}] Generation complete — ProposalId={ProposalId}",
                AlgorithmName, proposalId);
        }
        catch (OperationCanceledException)
        {
            Logger.LogWarning("[{Algo}] Generation cancelled — ProposalId={ProposalId}",
                AlgorithmName, proposalId);
            proposal.Status = ProposalStatus.Archived;
            await Db.SaveChangesAsync(CancellationToken.None);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[{Algo}] Generation failed — ProposalId={ProposalId}",
                AlgorithmName, proposalId);
            proposal.Status = ProposalStatus.Archived;
            await Db.SaveChangesAsync(CancellationToken.None);
            throw;
        }
    }

    // ── Abstract members ──────────────────────────────────────────────────────

    protected abstract string AlgorithmName { get; }

    protected abstract Task RunAlgorithmAsync(
        RosterProposal proposal,
        ProposalGenerationContext context,
        CancellationToken cancellationToken);

    // ── Shared data-fetching ──────────────────────────────────────────────────

    protected async Task<ProposalGenerationContext> FetchContextAsync(
        RosterProposal proposal,
        CancellationToken cancellationToken)
    {
        var tasks = await Db.Tasks
            .Where(t => t.TenantId == proposal.TenantId && t.IsActive)
            .Select(t => new AgentTaskDto(t.TaskId, t.TaskName, t.Frequency, t.DayRule, t.RequiredSkillId, t.IsRestricted))
            .ToListAsync(cancellationToken);

        var members = await Db.Users
            .Where(u => u.TenantId == proposal.TenantId && u.IsActive && u.Role == "Member")
            .Select(u => new AgentMemberDto(
                u.UserId,
                u.Name,
                u.MonthlyLimit,
                u.UserSkills.Select(us => us.SkillId).ToList()))
            .ToListAsync(cancellationToken);

        var startDt = DateTime.SpecifyKind(proposal.DateRangeStart.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var endDt   = DateTime.SpecifyKind(proposal.DateRangeEnd.ToDateTime(TimeOnly.MaxValue),   DateTimeKind.Utc);

        var existingAssignments = await Db.Assignments
            .IgnoreQueryFilters()
            .Where(a =>
                a.TenantId == proposal.TenantId &&
                a.EventDate >= startDt &&
                a.EventDate <= endDt &&
                a.Status != "Expired" &&
                a.Status != "Rejected")
            .Select(a => new AgentAssignmentDto(a.TaskId, a.UserId, a.EventDate.ToString("yyyy-MM-dd")))
            .ToListAsync(cancellationToken);

        var memberIds = members.Select(m => m.UserId).ToList();

        var assignmentCounts = await Db.Assignments
            .IgnoreQueryFilters()
            .Where(a =>
                memberIds.Contains(a.UserId) &&
                a.EventDate >= startDt &&
                a.EventDate <= endDt &&
                a.Status != "Expired" &&
                a.Status != "Rejected")
            .GroupBy(a => new { a.UserId, a.EventDate.Month, a.EventDate.Year })
            .Select(g => new AgentMemberMonthCountDto(g.Key.UserId, g.Key.Month, g.Key.Year, g.Count()))
            .ToListAsync(cancellationToken);

        var scheduledSlots = ComputeScheduledSlots(tasks, proposal.DateRangeStart, proposal.DateRangeEnd);

        Logger.LogInformation(
            "[{Algo}] Context fetched — Tasks={T}, Members={M}, ExistingAssignments={E}, Slots={S}",
            AlgorithmName, tasks.Count, members.Count, existingAssignments.Count, scheduledSlots.Count);

        return new ProposalGenerationContext(
            scheduledSlots, members, existingAssignments, assignmentCounts);
    }

    // ── Slot computation (shared, pure C# calendar logic) ────────────────────

    protected static List<AgentScheduledSlotDto> ComputeScheduledSlots(
        List<AgentTaskDto> tasks,
        DateOnly rangeStart,
        DateOnly rangeEnd)
    {
        var slots = new List<AgentScheduledSlotDto>();

        foreach (var task in tasks)
        {
            var dayRuleTrimmed = task.DayRule.Trim();
            var weekdayToken = dayRuleTrimmed.StartsWith("Last ", StringComparison.OrdinalIgnoreCase)
                ? dayRuleTrimmed["Last ".Length..].Trim()
                : dayRuleTrimmed;

            if (!Enum.TryParse<DayOfWeek>(weekdayToken, ignoreCase: true, out var targetDay))
                continue;

            var isWeekly = task.Frequency.Equals("Weekly", StringComparison.OrdinalIgnoreCase);

            if (isWeekly)
            {
                var date = rangeStart;
                var daysUntilTarget = ((int)targetDay - (int)date.DayOfWeek + 7) % 7;
                date = date.AddDays(daysUntilTarget);

                while (date <= rangeEnd)
                {
                    slots.Add(new AgentScheduledSlotDto(
                        task.TaskId, task.TaskName, task.RequiredSkillId, task.IsRestricted,
                        date.ToString("yyyy-MM-dd")));
                    date = date.AddDays(7);
                }
            }
            else
            {
                var year  = rangeStart.Year;
                var month = rangeStart.Month;

                while (true)
                {
                    var monthEnd  = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
                    var daysBack  = ((int)monthEnd.DayOfWeek - (int)targetDay + 7) % 7;
                    var candidate = monthEnd.AddDays(-daysBack);

                    if (candidate >= rangeStart && candidate <= rangeEnd)
                    {
                        slots.Add(new AgentScheduledSlotDto(
                            task.TaskId, task.TaskName, task.RequiredSkillId, task.IsRestricted,
                            candidate.ToString("yyyy-MM-dd")));
                    }

                    month++;
                    if (month > 12) { month = 1; year++; }
                    if (new DateOnly(year, month, 1) > rangeEnd) break;
                }
            }
        }

        return slots;
    }
}

/// <summary>Pre-fetched data passed to each algorithm implementation.</summary>
public record ProposalGenerationContext(
    List<AgentScheduledSlotDto> ScheduledSlots,
    List<AgentMemberDto> Members,
    List<AgentAssignmentDto> ExistingAssignments,
    List<AgentMemberMonthCountDto> AssignmentCounts
);
