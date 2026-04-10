using System.Threading.Channels;
using ChurchRoster.Application.DTOs.Assignments;
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
    private readonly IAssignmentService _assignmentService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ProposalService> _logger;

    public ProposalService(
        AppDbContext db,
        ITenantContext tenantContext,
        Channel<int> channel,
        IAssignmentService assignmentService,
        INotificationService notificationService,
        ILogger<ProposalService> logger)
    {
        _db = db;
        _tenantContext = tenantContext;
        _channel = channel;
        _assignmentService = assignmentService;
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
                item.Status = ProposalItemStatus.Skipped;
                item.SkipReason = "Assignment already exists for this task and date";

                _db.ProposalSkipLogs.Add(new ProposalSkipLog
                {
                    ProposalId = proposalId,
                    TaskId = item.TaskId,
                    EventDate = item.EventDate,
                    Reason = item.SkipReason,
                    LoggedAt = DateTime.UtcNow
                });

                skipped++;
                _logger.LogInformation(
                    "Publish: skipped TaskId={TaskId} on {Date} (conflict)", item.TaskId, item.EventDate);
                continue;
            }

            // Create live assignment
            var assignmentResult = await _assignmentService.CreateAssignmentAsync(
                new CreateAssignmentRequest(item.TaskId, item.UserId, eventDateUtc, false),
                proposal.CreatedByUserId);

            if (assignmentResult is not null)
            {
                // Fire-and-forget notification — publish must not fail if push fails
                _ = _notificationService.SendAssignmentNotificationAsync(assignmentResult.AssignmentId)
                    .ContinueWith(t =>
                        _logger.LogError(t.Exception, "Notification failed for AssignmentId={Id}", assignmentResult.AssignmentId),
                        System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                created++;
            }
            else
            {
                // Assignment creation failed (e.g. validation) — treat as skipped
                item.Status = ProposalItemStatus.Skipped;
                item.SkipReason = "Assignment creation failed — validation error";
                skipped++;
                _logger.LogWarning(
                    "Publish: assignment creation failed for TaskId={TaskId} on {Date}", item.TaskId, item.EventDate);
            }
        }

        proposal.Status = ProposalStatus.Published;
        proposal.PublishedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Proposal {ProposalId} published: {Created} assignments created, {Skipped} slots skipped",
            proposalId, created, skipped);

        return new PublishProposalResult(proposalId, created, skipped);
    }

    /// <summary>Archives a proposal (Draft or Published).</summary>
    public async Task<bool> ArchiveProposalAsync(int proposalId)
    {
        var proposal = await _db.RosterProposals
            .FirstOrDefaultAsync(p => p.ProposalId == proposalId);

        if (proposal is null) return false;
        if (proposal.Status == ProposalStatus.Processing) return false;

        proposal.Status = ProposalStatus.Archived;
        await _db.SaveChangesAsync();
        return true;
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
        new(log.LogId, log.TaskId, log.EventDate, log.Reason, log.LoggedAt);
}
