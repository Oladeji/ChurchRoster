using ChurchRoster.Application.Interfaces;
using ChurchRoster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChurchRoster.Api.BackgroundServices;

public class AssignmentStatusUpdateService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AssignmentStatusUpdateService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(6);

    public AssignmentStatusUpdateService(
        IServiceProvider serviceProvider,
        ILogger<AssignmentStatusUpdateService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Assignment Status Update Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var discoveryScope = _serviceProvider.CreateScope();
                var discoveryContext = discoveryScope.ServiceProvider.GetRequiredService<AppDbContext>();
                var tenants = await discoveryContext.Tenants
                    .IgnoreQueryFilters()
                    .Where(t => t.IsActive)
                    .AsNoTracking()
                    .ToListAsync(stoppingToken);

                var totalUpdated = 0;

                foreach (var tenant in tenants)
                {
                    using var tenantScope = _serviceProvider.CreateScope();
                    var tenantContext = tenantScope.ServiceProvider.GetRequiredService<ITenantContext>();
                    tenantContext.TenantId = tenant.TenantId;
                    tenantContext.TenantName = tenant.Name;

                    var assignmentService = tenantScope.ServiceProvider.GetRequiredService<IAssignmentService>();
                    var updatedCount = await assignmentService.AutoUpdatePastAssignmentStatusesAsync();
                    totalUpdated += updatedCount;

                    if (updatedCount > 0)
                    {
                        _logger.LogInformation("Assignment status updater changed {Count} past assignment statuses for tenant {TenantName}", updatedCount, tenant.Name);
                    }
                }

                if (totalUpdated > 0)
                {
                    _logger.LogInformation("Assignment status updater changed {Count} past assignment statuses across all tenants", totalUpdated);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running assignment status updater service");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
