using ChurchRoster.Application.Interfaces;
using ChurchRoster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChurchRoster.Api.BackgroundServices;

public class AssignmentReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AssignmentReminderService> _logger;
    private Timer? _timer;

    public AssignmentReminderService(
        IServiceProvider serviceProvider,
        ILogger<AssignmentReminderService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Assignment Reminder Service started");

        // Calculate time until next Sunday at 8:00 AM
        var now = DateTime.Now;
        var nextSunday = GetNextSunday(now);
        var timeUntilNextSunday = nextSunday - now;

        _logger.LogInformation("Next reminder scheduled for: {NextSunday}", nextSunday);
        _logger.LogInformation("Time until next reminder: {TimeSpan}", timeUntilNextSunday);

        // Wait until the first Sunday
        if (timeUntilNextSunday.TotalMilliseconds > 0)
        {
            await Task.Delay(timeUntilNextSunday, stoppingToken);
        }

        // Then run every week (7 days)
        _timer = new Timer(
            async _ => await SendReminders(stoppingToken),
            null,
            TimeSpan.Zero,
            TimeSpan.FromDays(7));

        await Task.CompletedTask;
    }

    private DateTime GetNextSunday(DateTime from)
    {
        var daysUntilSunday = ((int)DayOfWeek.Sunday - (int)from.DayOfWeek + 7) % 7;
        if (daysUntilSunday == 0 && from.Hour >= 8)
        {
            daysUntilSunday = 7; // If it's already Sunday after 8 AM, schedule for next Sunday
        }

        var nextSunday = from.AddDays(daysUntilSunday);
        return new DateTime(nextSunday.Year, nextSunday.Month, nextSunday.Day, 8, 0, 0);
    }

    private async Task SendReminders(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting automatic Sunday reminder process at {Time}", DateTime.Now);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var today = DateTime.UtcNow.Date;
            var eighteenDaysFromNow = today.AddDays(18);

            // Get all pending/accepted assignments within the next 18 days
            var upcomingAssignments = await context.Assignments
                .Include(a => a.User)
                .Include(a => a.Task)
                .Where(a => a.EventDate >= today && 
                           a.EventDate <= eighteenDaysFromNow &&
                           (a.Status == "Pending" || a.Status == "Accepted"))
                .ToListAsync(stoppingToken);

            _logger.LogInformation("Found {Count} upcoming assignments to send reminders for", upcomingAssignments.Count);

            foreach (var assignment in upcomingAssignments)
            {
                try
                {
                    var daysUntil = (assignment.EventDate.Date - today).Days;

                    await emailService.SendAssignmentReminderAsync(
                        assignment.User.Email,
                        assignment.User.Name,
                        assignment.Task.TaskName,
                        assignment.EventDate,
                        daysUntil);

                    _logger.LogInformation("Sent reminder for assignment {AssignmentId} to {UserEmail}",
                        assignment.AssignmentId, assignment.User.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send reminder for assignment {AssignmentId}", assignment.AssignmentId);
                }
            }

            _logger.LogInformation("Completed automatic Sunday reminder process");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in automatic Sunday reminder process");
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Assignment Reminder Service is stopping");
        _timer?.Change(Timeout.Infinite, 0);
        await base.StopAsync(stoppingToken);
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }
}
