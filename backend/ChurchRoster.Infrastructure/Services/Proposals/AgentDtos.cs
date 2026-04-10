namespace ChurchRoster.Infrastructure.Services.Proposals;

// Returned by GetRecurringTasksAsync — describes a task the agent must schedule
public record AgentTaskDto(
    int TaskId,
    string TaskName,
    string Frequency,   // "Weekly" or "Monthly"
    string DayRule,     // "Tuesday", "Sunday", "Last Friday", etc.
    int? RequiredSkillId,
    bool IsRestricted
);

// Returned by GetQualifiedMembersAsync — only UserId sent to LLM (anonymised)
public record AgentMemberDto(
    int UserId,
    int? MonthlyLimit
);

// Returned by GetExistingAssignmentsAsync — lets agent avoid conflicts
public record AgentAssignmentDto(
    int TaskId,
    int UserId,
    DateOnly EventDate
);
