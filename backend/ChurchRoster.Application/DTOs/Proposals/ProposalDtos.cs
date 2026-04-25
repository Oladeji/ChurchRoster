namespace ChurchRoster.Application.DTOs.Proposals;

// ── Requests ────────────────────────────────────────────────────────────────

public record GenerateProposalRequest(
    string Name,
    DateOnly DateRangeStart,
    DateOnly DateRangeEnd
);

public record UpdateProposalItemRequest(
    int NewUserId
);

public record AddProposalItemRequest(
    int TaskId,
    int UserId,
    DateOnly EventDate
);

// ── Response DTOs ────────────────────────────────────────────────────────────

public record ProposalSummaryDto(
    int ProposalId,
    string Name,
    string Status,
    DateOnly DateRangeStart,
    DateOnly DateRangeEnd,
    DateTime GeneratedAt,
    DateTime? PublishedAt,
    int ItemCount
);

public record ProposalItemDto(
    int ItemId,
    int TaskId,
    string TaskName,
    int UserId,
    string MemberName,
    DateOnly EventDate,
    string Status,
    string? SkipReason
);

public record SkipLogDto(
    int LogId,
    int TaskId,
    string TaskName,
    DateOnly EventDate,
    string Reason,
    DateTime LoggedAt
);

public record ProposalDetailDto(
    int ProposalId,
    string Name,
    string Status,
    DateOnly DateRangeStart,
    DateOnly DateRangeEnd,
    DateTime GeneratedAt,
    DateTime? PublishedAt,
    IEnumerable<ProposalItemDto> Items,
    IEnumerable<SkipLogDto> SkipLogs
);

public record GenerateProposalResult(
    int ProposalId,
    string Status
);

public record PublishSkippedItemDto(
    string TaskName,
    string MemberName,
    DateOnly EventDate,
    string Reason
);

public record PublishProposalResult(
    int ProposalId,
    int AssignmentsCreated,
    int SlotsSkipped,
    IReadOnlyList<PublishSkippedItemDto> Skipped
);
