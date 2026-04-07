namespace ChurchRoster.Infrastructure.Data;

public interface ITenantContext
{
    int? TenantId { get; set; }
    string? TenantName { get; set; }
    bool IsResolved => TenantId.HasValue;
}
