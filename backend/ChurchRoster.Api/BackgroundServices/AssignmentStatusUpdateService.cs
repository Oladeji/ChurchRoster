using ChurchRoster.Application.Interfaces;

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
                using var scope = _serviceProvider.CreateScope();
                var assignmentService = scope.ServiceProvider.GetRequiredService<IAssignmentService>();

                var updatedCount = await assignmentService.AutoUpdatePastAssignmentStatusesAsync();

                if (updatedCount > 0)
                {
                    _logger.LogInformation("Assignment status updater changed {Count} past assignment statuses", updatedCount);
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
