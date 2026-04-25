namespace ChurchRoster.Infrastructure.Services.Proposals;

// ── Prompt context DTOs (pre-fetched by C#, serialised into the prompt) ──────

// A recurring task the agent must schedule
public record AgentTaskDto(
    int TaskId,
    string TaskName,
    string Frequency,       // "Weekly" or "Monthly"
    string DayRule,         // "Tuesday", "Sunday", "Last Friday", etc.
    int? RequiredSkillId,
    bool IsRestricted
);

// A concrete slot C# computed from a task's Frequency+DayRule — the LLM only
// needs to assign a member, never do calendar arithmetic itself.
public record AgentScheduledSlotDto(
    int TaskId,
    string TaskName,
    int? RequiredSkillId,
    bool IsRestricted,
    string EventDate        // "YYYY-MM-DD" — exact date already resolved by C#
);

// A member the agent may assign
public record AgentMemberDto(
    int UserId,
    string Name,
    int? MonthlyLimit,
    List<int> SkillIds      // skill IDs this member holds
);

// An existing live assignment — used to detect conflicts
public record AgentAssignmentDto(
    int TaskId,
    int UserId,
    string EventDate        // "YYYY-MM-DD" — plain string so JSON serialization is simple
);

// Per-member assignment count already booked in a given month
public record AgentMemberMonthCountDto(
    int UserId,
    int Month,
    int Year,
    int Count
);

// ── Response DTOs (parsed from the LLM JSON reply) ───────────────────────────

// One assignment the LLM decided to create
public record AgentProposedItemDto(
    int TaskId,
    int UserId,
    string EventDate        // "YYYY-MM-DD"
);

// One slot the LLM could not fill
public record AgentSkippedSlotDto(
    int TaskId,
    string EventDate,       // "YYYY-MM-DD"
    string Reason
);

// The full structured response the LLM returns as JSON
public class AgentGenerationResponse
{
    public List<AgentProposedItemDto> Assignments { get; set; } = [];
    public List<AgentSkippedSlotDto> Skipped { get; set; } = [];
}
