namespace ChurchRoster.Application.Interfaces;

public interface IReportService
{
    Task<byte[]> GenerateMonthlyRosterAsync(int year, int month);
    Task<byte[]> GenerateMemberScheduleAsync(int userId, DateTime startDate, DateTime endDate);
    Task<byte[]> GenerateTaskAssignmentReportAsync(DateTime startDate, DateTime endDate);
}
