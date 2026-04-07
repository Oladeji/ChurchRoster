using ChurchRoster.Infrastructure.Data;

namespace ChurchRoster.Api.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        int? resolvedTenantId = null;

        var tenantClaim = context.User?.FindFirst("tenant_id")?.Value
            ?? context.User?.FindFirst("TenantId")?.Value;

        if (int.TryParse(tenantClaim, out var claimTenantId))
        {
            resolvedTenantId = claimTenantId;
        }
        else if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader) &&
                 int.TryParse(tenantHeader.FirstOrDefault(), out var headerTenantId))
        {
            resolvedTenantId = headerTenantId;
        }

        if (resolvedTenantId.HasValue)
        {
            tenantContext.TenantId = resolvedTenantId.Value;
            _logger.LogDebug("Resolved tenant {TenantId} for {Method} {Path}", resolvedTenantId.Value, context.Request.Method, context.Request.Path);
        }

        await _next(context);
    }
}
