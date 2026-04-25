using System.Threading.Channels;
using ChurchRoster.Application.DTOs.Proposals;
using ChurchRoster.Application.Interfaces;
using ChurchRoster.Core.Entities;
using ChurchRoster.Core.Entities.Proposals;
using ChurchRoster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChurchRoster.Application.Services;

public class ProposalService : IProposalService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly Channel<int> _channel;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ProposalService> _logger;

    public ProposalService(
        AppDbContext db,
        ITenantContext tenantContext,
        Channel<int> channel,
        INotificationService notificationService,
        ILogger<ProposalService> logger)
    {
        _db = db;
        _tenantContext = tenantContext;
        _channel = channel;
        _notificationService = notificationService;
        _logger = logger;
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates the proposal record (Status=Processing), enqueues it for the
    /// AI agent, and returns immediately with HTTP 202.
    /// Enforces one-in-flight per tenant (returns null → 409 in endpoint).
    /// </summary>
    public async Task<GenerateProposalResult?> GenerateProposalAsync(
        GenerateProposalRequest request, int createdByUserId)
    {
        if (!_tenantContext.TenantId.HasValue)
            return null;

        var tenantId = _tenantContext.TenantId.Value;

        // One-in-flight rule: reject if this tenant already has a Processing proposal
        var alreadyProcessing = await _db.RosterProposals
            .AnyAsync(p => p.TenantId == tenantId && p.Status == ProposalStatus.Processing);

        if (alreadyProcessing)
        {
            _logger.LogWarning(
                "GenerateProposal rejected — tenant {TenantId} already has a Processing proposal", tenantId);
            return null;
        }

        var proposal = new RosterProposal
        {
            TenantId = tenantId,
            Name = request.Name,
            Status = ProposalStatus.Processing,
            DateRangeStart = request.DateRangeStart,
            DateRangeEnd = request.DateRangeEnd,
            GeneratedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId
        };

        _db.RosterProposals.Add(proposal);
        await _db.SaveChangesAsync();

        // Enqueue for background job — non-blocking
        await _channel.Writer.WriteAsync(proposal.ProposalId);

        _logger.LogInformation(
            "Proposal {ProposalId} created and queued for generation (tenant {TenantId})",
            proposal.ProposalId, tenantId);

        return new GenerateProposalResult(proposal.ProposalId, proposal.Status.ToString());
    }

    /// <summary>Swaps the assigned member on a draft proposal item.</summary>
    public async Task<ProposalItemDto?> UpdateProposalItemAsync(
        int itemId, UpdateProposalItemRequest request)
    {
        var item = await _db.RosterProposalItems
            .Include(i => i.Proposal)
            .Include(i => i.Task)
            .Include(i => i.User)
            .FirstOrDefaultAsync(i => i.ItemId == itemId);

        if (item is null) return null;
        if (item.Proposal.Status != ProposalStatus.Draft) return null;

        var newUser = await _db.Users
            .FirstOrDefaultAsync(u => u.UserId == request.NewUserId);
        if (newUser is null) return null;

        item.UserId = request.NewUserId;
        item.Status = ProposalItemStatus.Proposed;
        item.SkipReason = null;
        await _db.SaveChangesAsync();

        return MapItemToDto(item, item.Task.TaskName, newUser.Name);
    }

    /// <summary>Adds a new item to a draft proposal.</summary>
    public async Task<ProposalItemDto?> AddProposalItemAsync(
        int proposalId, AddProposalItemRequest request)
    {
        var proposal = await _db.RosterProposals
            .FirstOrDefaultAsync(p => p.ProposalId == proposalId);

        if (proposal is null || proposal.Status != ProposalStatus.Draft)
            return null;

        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.TaskId == request.TaskId);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == request.UserId);

        if (task is null || user is null) return null;

        var item = new RosterProposalItem
        {
            ProposalId = proposalId,
            TaskId = request.TaskId,
            UserId = request.UserId,
            EventDate = request.EventDate,
            Status = ProposalItemStatus.Proposed
        };

        _db.RosterProposalItems.Add(item);
        await _db.SaveChangesAsync();

        return MapItemToDto(item, task.TaskName, user.Name);
    }

    /// <summary>Deletes a single item from a draft proposal.</summary>
    public async Task<bool> DeleteProposalItemAsync(int itemId)
    {
        var item = await _db.RosterProposalItems
            .Include(i => i.Proposal)
            .FirstOrDefaultAsync(i => i.ItemId == itemId);

        if (item is null) return false;
        if (item.Proposal.Status != ProposalStatus.Draft) return false;

        _db.RosterProposalItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Publishes a draft proposal to the live calendar.
    /// For each item: if a live Assignment already exists for the same
    /// tenant+task+date → mark Skipped and log. Otherwise create Assignment
    /// (Status=Pending) and send notification.
    /// Never fails the whole batch for a single conflict.
    /// </summary>
    public async Task<PublishProposalResult?> PublishProposalAsync(int proposalId)
    {
        if (!_tenantContext.TenantId.HasValue) return null;

        var proposal = await _db.RosterProposals
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.ProposalId == proposalId);

        if (proposal is null || proposal.Status != ProposalStatus.Draft)
            return null;

        var tenantId = _tenantContext.TenantId.Value;
        int created = 0;
        int skipped = 0;
        var skippedDetails = new List<PublishSkippedItemDto>();

        // Pre-load task and member names for skip detail messages
        var taskNames = await _db.Tasks
            .Where(t => proposal.Items.Select(i => i.TaskId).Contains(t.TaskId))
            .ToDictionaryAsync(t => t.TaskId, t => t.TaskName);

        var memberNames = await _db.Users
            .Where(u => proposal.Items.Select(i => i.UserId).Contains(u.UserId))
            .ToDictionaryAsync(u => u.UserId, u => u.Name);

        foreach (var item in proposal.Items.Where(i => i.Status == ProposalItemStatus.Proposed))
        {
            var eventDateUtc = item.EventDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

            // Check for existing live assignment: same tenant + task + date
            var conflict = await _db.Assignments
                .AnyAsync(a =>
                    a.TenantId == tenantId &&
                    a.TaskId == item.TaskId &&
                    a.EventDate == eventDateUtc &&
                    a.Status != "Expired" &&
                    a.Status != "Rejected");

            if (conflict)
            {
                var reason = "Assignment already exists for this task and date";
                item.Status = ProposalItemStatus.Skipped;
                item.SkipReason = reason;

                _db.ProposalSkipLogs.Add(new ProposalSkipLog
                {
                    ProposalId = proposalId,
                    TaskId = item.TaskId,
                    EventDate = item.EventDate,
                    Reason = reason,
                    LoggedAt = DateTime.UtcNow
                });

                skippedDetails.Add(new PublishSkippedItemDto(
                    taskNames.GetValueOrDefault(item.TaskId, $"Task #{item.TaskId}"),
                    memberNames.GetValueOrDefault(item.UserId, $"Member #{item.UserId}"),
                    item.EventDate,
                    reason));

                skipped++;
                _logger.LogInformation(
                    "Publish: skipped TaskId={TaskId} on {Date} (conflict)", item.TaskId, item.EventDate);
                continue;
            }

            // Create the live assignment directly — validation rules (past date,
            // same-day per-user, skill check) were enforced at generation time and
            // reviewed by the admin before publish. Bypassing them here prevents
            // legitimate items from being silently skipped because time has passed
            // or the user already has a different task on the same day.
            var assignment = new Assignment
            {
                TenantId   = tenantId,
                TaskId     = item.TaskId,
                UserId     = item.UserId,
                EventDate  = eventDateUtc,
                Status     = "Pending",
                IsOverride = false,
                AssignedBy = proposal.CreatedByUserId,
                CreatedAt  = DateTime.UtcNow,
                UpdatedAt  = DateTime.UtcNow,
            };
            _db.Assignments.Add(assignment);
            await _db.SaveChangesAsync();

            try
            {
                await _notificationService.SendAssignmentNotificationAsync(assignment.AssignmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notification failed for AssignmentId={Id}", assignment.AssignmentId);
            }
            created++;
        }

        proposal.Status = ProposalStatus.Published;
        proposal.PublishedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Proposal {ProposalId} published: {Created} assignments created, {Skipped} slots skipped",
            proposalId, created, skipped);

        return new PublishProposalResult(proposalId, created, skipped, skippedDetails);
    }

    /// <summary>Archives a proposal (any status except already Archived).</summary>
    public async Task<bool> ArchiveProposalAsync(int proposalId)
    {
        var proposal = await _db.RosterProposals
            .FirstOrDefaultAsync(p => p.ProposalId == proposalId);

        if (proposal is null) return false;
        if (proposal.Status == ProposalStatus.Archived) return false;

        proposal.Status = ProposalStatus.Archived;
        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Archives the specified proposal (regardless of its current status) and
    /// immediately re-queues a fresh generation covering the same date range.
    /// Useful when a generation failed or got stuck in Processing.
    /// Returns null if the proposal does not exist.
    /// </summary>
    public async Task<GenerateProposalResult?> RetryProposalAsync(int proposalId, int createdByUserId)
    {
        var existing = await _db.RosterProposals
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.ProposalId == proposalId);

        if (existing is null)
        {
            _logger.LogWarning("RetryProposal: ProposalId={ProposalId} not found", proposalId);
            return null;
        }

        // Archive the stuck/failed proposal so the one-in-flight guard does not block us
        existing.Status = ProposalStatus.Archived;
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "RetryProposal: ProposalId={ProposalId} archived — re-queuing with same range {Start} to {End}",
            proposalId, existing.DateRangeStart, existing.DateRangeEnd);

        return await GenerateProposalAsync(
            new GenerateProposalRequest(existing.Name, existing.DateRangeStart, existing.DateRangeEnd),
            createdByUserId);
    }

    // ── Queries ──────────────────────────────────────────────────────────────

    public async Task<ProposalDetailDto?> GetProposalByIdAsync(int proposalId)
    {
        var proposal = await _db.RosterProposals
            .Include(p => p.Items)
                .ThenInclude(i => i.Task)
            .Include(p => p.Items)
                .ThenInclude(i => i.User)
            .Include(p => p.SkipLogs)
                .ThenInclude(l => l.Task)
            .FirstOrDefaultAsync(p => p.ProposalId == proposalId);

        if (proposal is null) return null;

        return new ProposalDetailDto(
            proposal.ProposalId,
            proposal.Name,
            proposal.Status.ToString(),
            proposal.DateRangeStart,
            proposal.DateRangeEnd,
            proposal.GeneratedAt,
            proposal.PublishedAt,
            proposal.Items.Select(i => MapItemToDto(i, i.Task.TaskName, i.User.Name)),
            proposal.SkipLogs.Select(MapSkipLogToDto)
        );
    }

    public async Task<IEnumerable<ProposalSummaryDto>> GetProposalListAsync()
    {
        return await _db.RosterProposals
            .OrderByDescending(p => p.GeneratedAt)
            .Select(p => new ProposalSummaryDto(
                p.ProposalId,
                p.Name,
                p.Status.ToString(),
                p.DateRangeStart,
                p.DateRangeEnd,
                p.GeneratedAt,
                p.PublishedAt,
                p.Items.Count))
            .ToListAsync();
    }

    // ── Mappers ──────────────────────────────────────────────────────────────

    private static ProposalItemDto MapItemToDto(
        RosterProposalItem item, string taskName, string memberName) =>
        new(item.ItemId, item.TaskId, taskName,
            item.UserId, memberName,
            item.EventDate, item.Status.ToString(), item.SkipReason);

    private static SkipLogDto MapSkipLogToDto(ProposalSkipLog log) =>
        new(log.LogId, log.TaskId, log.Task.TaskName, log.EventDate, log.Reason, log.LoggedAt);
}
