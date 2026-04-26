using ChurchRoster.Application.Interfaces;
using ChurchRoster.Infrastructure.Data;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static Google.Apis.Requests.BatchRequest;

namespace ChurchRoster.Application.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationService> _logger;
    private readonly FirebaseMessaging? _messaging;

    public NotificationService(
        AppDbContext context,
        IConfiguration configuration,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;

        try
        {
            // Initialize Firebase Admin SDK
            if (FirebaseApp.DefaultInstance == null)
            {
                var serviceAccountJson = configuration["Firebase:ServiceAccountJson"];
                var serviceAccountPath = configuration["Firebase:ServiceAccountPath"];

                GoogleCredential? credential = null;

                if (!string.IsNullOrEmpty(serviceAccountJson))
                {
                    // Render env vars can corrupt \n in private keys into literal "\n" text.
                    // Replace escaped newlines back to real newlines so the key parses correctly.
                    serviceAccountJson = serviceAccountJson.Replace("\\n", "\n");
                    credential = GoogleCredential.FromJson(serviceAccountJson);
                    _logger.LogInformation("Firebase initialized from ServiceAccountJson");
                }
                else if (!string.IsNullOrEmpty(serviceAccountPath) && File.Exists(serviceAccountPath))
                {
                    // Option 2: JSON file path
                    credential = GoogleCredential.FromFile(serviceAccountPath);
                    _logger.LogInformation("Firebase initialized from ServiceAccountPath: {Path}", serviceAccountPath);
                }

                if (credential != null)
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = credential,
                        ProjectId = configuration["Firebase:ProjectId"]
                    });

                    _messaging = FirebaseMessaging.DefaultInstance;
                    _logger.LogInformation("Firebase Messaging initialized successfully");
                }
                else
                {
                    _logger.LogWarning(
                        "Firebase service account configuration not found. " +
                        "Push notifications will be disabled. " +
                        "Set either Firebase:ServiceAccountJson or Firebase:ServiceAccountPath in configuration.");
                }
            }
            else
            {
                _messaging = FirebaseMessaging.DefaultInstance;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Firebase. Push notifications will be disabled.");
        }
    }

    public async Task<bool> SendAssignmentNotificationAsync(int assignmentId)
    {
        if (_messaging == null)
        {
            _logger.LogWarning("Firebase not configured. Skipping notification for assignment {AssignmentId}", assignmentId);
            return false;
        }

        try
        {
            var assignment = await _context.Assignments
                .Include(a => a.Task)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment == null)
            {
                _logger.LogWarning("Assignment {AssignmentId} not found", assignmentId);
                return false;
            }

            if (string.IsNullOrEmpty(assignment.User.DeviceToken))
            {
                _logger.LogInformation("User {UserId} has no device token. Skipping notification.", assignment.UserId);
                return false;
            }

            var title = "New Ministry Assignment";
            var body = $"You've been assigned to: {assignment.Task.TaskName} on {assignment.EventDate:MMM dd, yyyy}";

            var data = new Dictionary<string, string>
            {
                { "type", "new_assignment" },
                { "assignmentId", assignmentId.ToString() },
                { "taskName", assignment.Task.TaskName },
                { "eventDate", assignment.EventDate.ToString("yyyy-MM-dd") }
            };

            return await SendCustomNotificationAsync(assignment.User.DeviceToken, title, body, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send assignment notification for {AssignmentId}", assignmentId);
            return false;
        }
    }

    // Overload that doesn't require database access - use this for fire-and-forget notifications
    public async Task<bool> SendAssignmentNotificationAsync(string deviceToken, string userName, string taskName, DateTime eventDate, int assignmentId)
    {
        if (_messaging == null)
        {
            _logger.LogWarning("Firebase not configured. Skipping notification");
            return false;
        }

        if (string.IsNullOrEmpty(deviceToken))
        {
            _logger.LogInformation("User {UserName} has no device token. Skipping notification.", userName);
            return false;
        }

        try
        {
            var title = "New Ministry Assignment";
            var body = $"You've been assigned to: {taskName} on {eventDate:MMM dd, yyyy}";

            var data = new Dictionary<string, string>
            {
                { "type", "new_assignment" },
                { "assignmentId", assignmentId.ToString() },
                { "taskName", taskName },
                { "eventDate", eventDate.ToString("yyyy-MM-dd") }
            };

            return await SendCustomNotificationAsync(deviceToken, title, body, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send assignment notification for assignment {AssignmentId}", assignmentId);
            return false;
        }
    }

    public async Task<bool> SendStatusUpdateNotificationAsync(int assignmentId, string newStatus)
    {
        if (_messaging == null)
        {
            _logger.LogWarning("Firebase not configured. Skipping status update notification");
            return false;
        }

        try
        {
            var assignment = await _context.Assignments
                .Include(a => a.Task)
                .Include(a => a.User)
                .Include(a => a.AssignedByUser)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment?.AssignedByUser == null)
            {
                _logger.LogWarning("Assignment {AssignmentId} or admin user not found", assignmentId);
                return false;
            }

            if (string.IsNullOrEmpty(assignment.AssignedByUser.DeviceToken))
            {
                _logger.LogInformation("Admin user {UserId} has no device token. Skipping notification.", assignment.AssignedBy);
                return false;
            }

            var title = "Assignment Status Updated";
            var body = $"{assignment.User.Name} has {newStatus.ToLower()} the {assignment.Task.TaskName} assignment";

            var data = new Dictionary<string, string>
            {
                { "type", "status_update" },
                { "assignmentId", assignmentId.ToString() },
                { "status", newStatus },
                { "memberName", assignment.User.Name }
            };

            return await SendCustomNotificationAsync(assignment.AssignedByUser.DeviceToken, title, body, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send status update notification for {AssignmentId}", assignmentId);
            return false;
        }
    }

    public async Task<bool> SendReminderNotificationAsync(int assignmentId)
    {
        if (_messaging == null)
        {
            _logger.LogWarning("Firebase not configured. Skipping reminder notification");
            return false;
        }

        try
        {
            var assignment = await _context.Assignments
                .Include(a => a.Task)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment == null)
            {
                _logger.LogWarning("Assignment {AssignmentId} not found", assignmentId);
                return false;
            }

            if (string.IsNullOrEmpty(assignment.User.DeviceToken))
            {
                _logger.LogInformation("User {UserId} has no device token. Skipping reminder.", assignment.UserId);
                return false;
            }

            var title = "Ministry Reminder";
            var body = $"Reminder: {assignment.Task.TaskName} is tomorrow ({assignment.EventDate:MMM dd})";

            var data = new Dictionary<string, string>
            {
                { "type", "reminder" },
                { "assignmentId", assignmentId.ToString() },
                { "taskName", assignment.Task.TaskName },
                { "eventDate", assignment.EventDate.ToString("yyyy-MM-dd") }
            };

            return await SendCustomNotificationAsync(assignment.User.DeviceToken, title, body, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send reminder notification for {AssignmentId}", assignmentId);
            return false;
        }
    }

    public async Task<bool> SendCustomNotificationAsync(
        string deviceToken,
        string title,
        string body,
        Dictionary<string, string>? data = null)
    {
        if (_messaging == null)
        {
            _logger.LogWarning("Firebase not configured. Skipping custom notification");
            return false;
        }

        try
        {
            var frontendUrl = _configuration["App:FrontendUrl"] ?? "http://localhost:3000";
            var absoluteUrl = BuildAbsoluteUrl(data?.ContainsKey("assignmentId") == true
                ? "/my-assignments"
                : "/");

            var webpushConfig = new WebpushConfig
            {
                Notification = new WebpushNotification
                {
                    Title = title,
                    Body = body,
                    Icon = "/icons/icon-192x192.png",
                    Badge = "/icons/badge-72x72.png",
                    RequireInteraction = true
                }
            };

            // Only set FcmOptions if using HTTPS (Firebase requires HTTPS URLs)
            if (absoluteUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                webpushConfig.FcmOptions = new WebpushFcmOptions
                {
                    Link = absoluteUrl
                };
            }

            var message = new Message
            {
                Token = deviceToken,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data,
                Webpush = webpushConfig
            };
            _logger.LogInformation("The message to be sent is : {Message}       ", message);
            var response = await _messaging.SendAsync(message);
            _logger.LogInformation("Notification sent successfully. Response: {Response}", response);
            return true;
        }
        catch (FirebaseMessagingException ex)
        {
            _logger.LogError(ex, "Firebase messaging error: {ErrorCode} | Message: {Message}", ex.MessagingErrorCode, ex.Message);

            if (ex.MessagingErrorCode == MessagingErrorCode.InvalidArgument ||
                ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
            {
                _logger.LogWarning("Invalid device token. Should be removed from user: {Token}", deviceToken);
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification: {Message} | InnerException: {Inner}", ex.Message, ex.InnerException?.Message);
            return false;
        }
    }

    private string BuildAbsoluteUrl(string relativePath)
    {
        var frontendUrl = _configuration["App:FrontendUrl"] ?? "http://localhost:3000";
        // Remove trailing slash from base URL if present
        frontendUrl = frontendUrl.TrimEnd('/');
        // Ensure relative path starts with /
        if (!relativePath.StartsWith('/'))
        {
            relativePath = "/" + relativePath;
        }
        return $"{frontendUrl}{relativePath}";
    }
}
