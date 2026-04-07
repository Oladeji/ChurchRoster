namespace ChurchRoster.Infrastructure.Data;

public class TenantContext : ITenantContext
{
    public int? TenantId { get; set; }
    public string? TenantName { get; set; }
}
