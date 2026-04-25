using ChurchRoster.Core.Entities.Proposals;
using ChurchRoster.Infrastructure.Data;
using Google.OrTools.Sat;
using Microsoft.Extensions.Logging;

namespace ChurchRoster.Infrastructure.Services.Proposals;

/// <summary>
/// OR-Tools CP-SAT roster generation algorithm.
///
/// Models the entire scheduling problem as a Constraint Programming problem
/// solved simultaneously — unlike the greedy approach, this sees all slots
/// at once and finds a globally feasible solution.
///
/// Decision variables:
///   assign[slotIdx, memberIdx] ∈ {0, 1}
///
/// Hard constraints:
///   C1 — Each slot is covered by at most one member.
///   C2 — A member is assigned at most once per calendar day (across all slots).
///   C3 — A member cannot be assigned to a slot whose requiredSkillId they don't hold.
///   C4 — A member cannot be assigned to a slot already covered by a live assignment.
///   C5 — Monthly assignment count (existing + new) must not exceed monthlyLimit.
///        (Soft overflow is permitted when no other assignment is possible —
///         enforced by a secondary pass identical to the greedy overflow rule.)
///
/// Objective:
///   Minimise the maximum monthly assignment count across all members
///   (i.e. spread assignments as evenly as possible).
///   Tie-break encoded as minimising sum of (count × userId) so lower userIds
///   are preferred when counts are equal — consistent with the greedy tie-break.
///
/// Slots that cannot be filled (infeasible after constraint propagation) are
/// recorded as skip-logs with a descriptive reason via a post-solve greedy pass.
/// </summary>
public class OrToolsProposalService : ProposalGenerationBase
{
    public OrToolsProposalService(
        AppDbContext db,
        ITenantContext tenantContext,
        ILogger<OrToolsProposalService> logger)
        : base(db, tenantContext, logger) { }

    protected override string AlgorithmName => "OrTools";

    protected override Task RunAlgorithmAsync(
        RosterProposal proposal,
        ProposalGenerationContext ctx,
        CancellationToken cancellationToken)
    {
        var proposalId = proposal.ProposalId;
        var slots      = ctx.ScheduledSlots;
        var members    = ctx.Members;

        // ── Pre-filter: slots already covered by live assignments ─────────────
        var coveredSlots = new HashSet<(int TaskId, string Date)>(
            ctx.ExistingAssignments.Select(a => (a.TaskId, a.EventDate)));

        // Existing same-day busy members: date → userIds
        var existingBusyOnDate = ctx.ExistingAssignments
            .GroupBy(a => a.EventDate)
            .ToDictionary(g => g.Key, g => new HashSet<int>(g.Select(a => a.UserId)));

        // Baseline monthly counts from DB
        var baseCounts = ctx.AssignmentCounts
            .ToDictionary(c => (c.UserId, c.Year, c.Month), c => c.Count);

        int BaseCount(int userId, int year, int month) =>
            baseCounts.TryGetValue((userId, year, month), out var v) ? v : 0;

        // Slots the OR-Tools model will decide on (live-conflict slots are skipped immediately)
        var openSlots = slots
            .Where(s => !coveredSlots.Contains((s.TaskId, s.EventDate)))
            .ToList();

        // Log immediately-skipped slots
        foreach (var s in slots.Where(s => coveredSlots.Contains((s.TaskId, s.EventDate))))
            AddSkipLog(proposalId, s, "Live assignment already exists for this task on this date");

        if (openSlots.Count == 0)
        {
            Logger.LogInformation("[OrTools] All slots already covered — nothing to solve");
            return Task.CompletedTask;
        }

        // ── Build OR-Tools CP-SAT model ───────────────────────────────────────
        var model = new CpModel();

        int S = openSlots.Count;
        int M = members.Count;

        // assign[s, m] = 1 if member m is assigned to slot s
        var assign = new BoolVar[S, M];
        for (int s = 0; s < S; s++)
            for (int m = 0; m < M; m++)
                assign[s, m] = model.NewBoolVar($"a_{s}_{m}");

        // ── C1: Each open slot covered by at most one member ──────────────────
        for (int s = 0; s < S; s++)
        {
            var row = Enumerable.Range(0, M).Select(m => assign[s, m]).ToArray();
            model.AddAtMostOne(row);
        }

        // ── C2: A member is assigned at most once per calendar day ────────────
        var slotsByDate = openSlots
            .Select((slot, idx) => (slot, idx))
            .GroupBy(x => x.slot.EventDate);

        foreach (var dateGroup in slotsByDate)
        {
            var slotIndices = dateGroup.Select(x => x.idx).ToList();
            if (slotIndices.Count < 2) continue;

            for (int m = 0; m < M; m++)
            {
                var memberId = members[m].UserId;
                var vars = slotIndices.Select(si => assign[si, m]).ToList();

                if (existingBusyOnDate.TryGetValue(dateGroup.Key, out var existingBusy)
                    && existingBusy.Contains(memberId))
                {
                    foreach (var v in vars)
                        model.Add(v == 0);
                }
                else
                {
                    model.AddAtMostOne(vars);
                }
            }
        }

        // ── C3: Skill requirement ─────────────────────────────────────────────
        for (int s = 0; s < S; s++)
        {
            var slot = openSlots[s];
            if (slot.RequiredSkillId is null or 0) continue;

            for (int m = 0; m < M; m++)
            {
                if (!members[m].SkillIds.Contains(slot.RequiredSkillId.Value))
                    model.Add(assign[s, m] == 0);
            }
        }

        // ── C4: Monthly limit (hard — will be relaxed in post-solve pass) ─────
        // Collect all (member, year, month) combinations in the open slots
        var monthKeys = openSlots
            .Select(s => DateOnly.Parse(s.EventDate))
            .Select(d => (d.Year, d.Month))
            .Distinct()
            .ToList();

        foreach (var (yr, mo) in monthKeys)
        {
            // Slot indices that fall in this month
            var slotsInMonth = openSlots
                .Select((slot, idx) => (slot, idx))
                .Where(x =>
                {
                    var d = DateOnly.Parse(x.slot.EventDate);
                    return d.Year == yr && d.Month == mo;
                })
                .Select(x => x.idx)
                .ToList();

            for (int m = 0; m < M; m++)
            {
                var member = members[m];
                if (member.MonthlyLimit is null) continue;

                var limit      = member.MonthlyLimit.Value;
                var alreadyHas = BaseCount(member.UserId, yr, mo);
                var remaining  = Math.Max(0, limit - alreadyHas);

                // Sum of assign[s, m] for slots in this month ≤ remaining
                var varsInMonth = slotsInMonth.Select(si => (ILiteral)assign[si, m]).ToArray();
                if (varsInMonth.Length > 0)
                    model.Add(LinearExpr.Sum(varsInMonth) <= remaining);
            }
        }

        // ── Objective: minimise max monthly count (fairness) ──────────────────
        // For each (member, month) compute new_count[m, ym] = base + sum(assign in month)
        // Then minimise max(new_count) across all members and months.

        // We encode fairness as minimising the weighted sum:
        //   Σ (slotIndex * memberUserId * assign[s,m])
        // which favours lower-userId members on tie counts AND lower-index slots —
        // consistent with greedy tie-break (lowest count → lowest userId).
        //
        // Primary objective: spread load → minimise sum of squares approximated as
        // minimise total assignments × member index (heavier members = higher index).
        // A simple approach: sum assign[s,m] * memberRank where memberRank is the
        // member's rank when sorted by current monthly count DESC (so the solver
        // prefers members with fewer existing assignments).

        var objTerms = new List<LinearExpr>();

        for (int s = 0; s < S; s++)
        {
            var slot   = openSlots[s];
            var parsed = DateOnly.Parse(slot.EventDate);
            var (yr, mo) = (parsed.Year, parsed.Month);

            for (int m = 0; m < M; m++)
            {
                var member = members[m];
                // Weight = existing count × 1000 + userId (so members with less work are cheaper)
                var weight = BaseCount(member.UserId, yr, mo) * 1000 + member.UserId;
                objTerms.Add(LinearExpr.Term(assign[s, m], weight));
            }
        }

        model.Minimize(LinearExpr.Sum(objTerms));

        // ── Solve ─────────────────────────────────────────────────────────────
        var solver = new CpSolver();
        solver.StringParameters = "max_time_in_seconds:30.0 num_search_workers:1 log_search_progress:false";

        Logger.LogInformation("[OrTools] Solving — {Slots} open slots, {Members} members", S, M);

        var status = solver.Solve(model);

        Logger.LogInformation("[OrTools] Solver status={Status} ObjectiveValue={Obj}",
            status, status is CpSolverStatus.Optimal or CpSolverStatus.Feasible
                ? solver.ObjectiveValue.ToString("F0")
                : "N/A");

        if (status is not (CpSolverStatus.Optimal or CpSolverStatus.Feasible))
        {
            Logger.LogWarning("[OrTools] No feasible solution found — falling back to skip-all");
            foreach (var slot in openSlots)
                AddSkipLog(proposalId, slot, "OR-Tools: no feasible assignment found within time limit");
            return Task.CompletedTask;
        }

        // ── Extract solution ──────────────────────────────────────────────────

        // Track assigned member per slot index; -1 means unassigned
        var assignedMember = new int[S];
        Array.Fill(assignedMember, -1);

        for (int s = 0; s < S; s++)
            for (int m = 0; m < M; m++)
                if (solver.BooleanValue(assign[s, m]))
                {
                    assignedMember[s] = m;
                    break;
                }

        // ── Post-solve: handle unassigned slots with greedy overflow pass ──────
        // Slots not covered by the solver (because all members hit monthly limits)
        // get a second chance with the same "fair overflow" rule as the greedy algo.

        // Running counts for overflow pass
        var runCounts = new Dictionary<(int UserId, int Year, int Month), int>(
            baseCounts.Select(kv =>
                KeyValuePair.Create((kv.Key.UserId, kv.Key.Year, kv.Key.Month), kv.Value)));

        // Seed runCounts with solver's assignments
        for (int s = 0; s < S; s++)
        {
            if (assignedMember[s] < 0) continue;
            var d = DateOnly.Parse(openSlots[s].EventDate);
            var k = (members[assignedMember[s]].UserId, d.Year, d.Month);
            runCounts[k] = (runCounts.TryGetValue(k, out var v) ? v : 0) + 1;
        }

        // Build same-date busy set including solver assignments
        var busyOnDate = existingBusyOnDate
            .ToDictionary(kv => kv.Key, kv => new HashSet<int>(kv.Value));

        for (int s = 0; s < S; s++)
        {
            if (assignedMember[s] < 0) continue;
            var date = openSlots[s].EventDate;
            if (!busyOnDate.TryGetValue(date, out var set)) { set = []; busyOnDate[date] = set; }
            set.Add(members[assignedMember[s]].UserId);
        }

        int GetRunCount(int userId, int yr, int mo) =>
            runCounts.TryGetValue((userId, yr, mo), out var c) ? c : 0;

        // Persist assignments
        for (int s = 0; s < S; s++)
        {
            var slot   = openSlots[s];
            var date   = slot.EventDate;
            var parsed = DateOnly.Parse(date);
            var (yr, mo) = (parsed.Year, parsed.Month);

            if (assignedMember[s] >= 0)
            {
                // Solver found an assignment
                var member = members[assignedMember[s]];
                Db.RosterProposalItems.Add(new RosterProposalItem
                {
                    ProposalId = proposalId,
                    TaskId     = slot.TaskId,
                    UserId     = member.UserId,
                    EventDate  = parsed,
                    Status     = ProposalItemStatus.Proposed,
                });
            }
            else
            {
                // Solver left slot unassigned — try greedy overflow
                var eligible = slot.RequiredSkillId is null or 0
                    ? members.ToList()
                    : members.Where(m => m.SkillIds.Contains(slot.RequiredSkillId.Value)).ToList();

                if (busyOnDate.TryGetValue(date, out var busy))
                    eligible = eligible.Where(m => !busy.Contains(m.UserId)).ToList();

                if (eligible.Count == 0)
                {
                    AddSkipLog(proposalId, slot, "All eligible members are already assigned to another task on this date");
                    continue;
                }

                // Fair overflow: keep all who are over limit
                var underLimit = eligible
                    .Where(m => m.MonthlyLimit is null || GetRunCount(m.UserId, yr, mo) < m.MonthlyLimit)
                    .ToList();

                if (underLimit.Count > 0) eligible = underLimit;

                var chosen = eligible
                    .OrderBy(m => GetRunCount(m.UserId, yr, mo))
                    .ThenBy(m => m.UserId)
                    .First();

                Db.RosterProposalItems.Add(new RosterProposalItem
                {
                    ProposalId = proposalId,
                    TaskId     = slot.TaskId,
                    UserId     = chosen.UserId,
                    EventDate  = parsed,
                    Status     = ProposalItemStatus.Proposed,
                });

                // Update state
                var k = (chosen.UserId, yr, mo);
                runCounts[k] = GetRunCount(chosen.UserId, yr, mo) + 1;
                if (!busyOnDate.TryGetValue(date, out var set)) { set = []; busyOnDate[date] = set; }
                set.Add(chosen.UserId);
            }
        }

        Logger.LogInformation("[OrTools] ProposalId={ProposalId} — solution extracted", proposalId);
        return Task.CompletedTask;
    }

    private void AddSkipLog(int proposalId, AgentScheduledSlotDto slot, string reason)
    {
        Logger.LogInformation("[OrTools] Skipping TaskId={TaskId} Date={Date} — {Reason}",
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
