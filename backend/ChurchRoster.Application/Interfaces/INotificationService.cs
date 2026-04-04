namespace ChurchRoster.Application.Interfaces;

public interface INotificationService
{
    Task<bool> SendAssignmentNotificationAsync(int assignmentId);
    Task<bool> SendAssignmentNotificationAsync(string deviceToken, string userName, string taskName, DateTime eventDate, int assignmentId);
    Task<bool> SendStatusUpdateNotificationAsync(int assignmentId, string newStatus);
    Task<bool> SendReminderNotificationAsync(int assignmentId);
    Task<bool> SendCustomNotificationAsync(string deviceToken, string title, string body, Dictionary<string, string>? data = null);
}
