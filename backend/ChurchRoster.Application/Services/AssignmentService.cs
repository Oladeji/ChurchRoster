using ChurchRoster.Application.DTOs.Assignments;
using ChurchRoster.Application.Interfaces;
using ChurchRoster.Core.Entities;
using ChurchRoster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChurchRoster.Application.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<AssignmentService> _logger;

        public AssignmentService(
            AppDbContext context, 
            IEmailService emailService,
            ILogger<AssignmentService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<IEnumerable<AssignmentDto>> GetAllAssignmentsAsync()
        {
            var assignments = await _context.Assignments
                .Include(a => a.Task)
                .Include(a => a.User)
                .Include(a => a.AssignedByUser)
                .OrderByDescending(a => a.EventDate)
                .ToListAsync();

            return assignments.Select(MapToDto);
        }

        public async Task<AssignmentDto?> GetAssignmentByIdAsync(int assignmentId)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Task)
                .Include(a => a.User)
                .Include(a => a.AssignedByUser)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            return assignment == null ? null : MapToDto(assignment);
        }

        public async Task<AssignmentDto?> CreateAssignmentAsync(CreateAssignmentRequest request, int assignedByUserId)
        {
            // Ensure EventDate is in UTC
            var eventDateUtc = request.EventDate.Kind == DateTimeKind.Utc 
                ? request.EventDate 
                : DateTime.SpecifyKind(request.EventDate, DateTimeKind.Utc);

            // Validate the assignment
            var validation = await ValidateAssignmentAsync(request.TaskId, request.UserId, eventDateUtc, request.IsOverride);

            if (!validation.IsValid && !request.IsOverride)
            {
                return null; // Validation failed and not overridden
            }

            var assignment = new Assignment
            {
                TaskId = request.TaskId,
                UserId = request.UserId,
                EventDate = eventDateUtc,
                Status = "Pending",
                IsOverride = request.IsOverride,
                AssignedBy = assignedByUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            // Send email notification to the assigned user
            try
            {
                var createdAssignment = await GetAssignmentByIdAsync(assignment.AssignmentId);
                if (createdAssignment != null)
                {
                    var user = await _context.Users.FindAsync(request.UserId);
                    var task = await _context.Tasks.FindAsync(request.TaskId);

                    if (user != null && task != null)
                    {
                        await _emailService.SendAssignmentNotificationAsync(
                            user.Email,
                            user.Name,
                            task.TaskName,
                            eventDateUtc);

                        _logger.LogInformation("Sent assignment notification email to {UserEmail}", user.Email);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send assignment notification email, but assignment was created");
            }

            return await GetAssignmentByIdAsync(assignment.AssignmentId);
        }

        public async Task<AssignmentDto?> UpdateAssignmentStatusAsync(int assignmentId, UpdateAssignmentStatusRequest request)
        {
            var assignment = await _context.Assignments.FindAsync(assignmentId);
            if (assignment == null)
                return null;

            assignment.Status = request.Status;
            assignment.RejectionReason = request.RejectionReason;
            assignment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetAssignmentByIdAsync(assignmentId);
        }

        public async Task<bool> DeleteAssignmentAsync(int assignmentId)
        {
            var assignment = await _context.Assignments.FindAsync(assignmentId);
            if (assignment == null)
                return false;

            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<AssignmentDto>> GetAssignmentsByUserAsync(int userId)
        {
            var assignments = await _context.Assignments
                .Include(a => a.Task)
                .Include(a => a.User)
                .Include(a => a.AssignedByUser)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.EventDate)
                .ToListAsync();

            return assignments.Select(MapToDto);
        }

        public async Task<IEnumerable<AssignmentDto>> GetAssignmentsByDateAsync(DateTime eventDate)
        {
            // Ensure eventDate is in UTC for comparison
            var eventDateUtc = eventDate.Kind == DateTimeKind.Utc 
                ? eventDate 
                : DateTime.SpecifyKind(eventDate, DateTimeKind.Utc);

            var assignments = await _context.Assignments
                .Include(a => a.Task)
                .Include(a => a.User)
                .Include(a => a.AssignedByUser)
                .Where(a => a.EventDate.Date == eventDateUtc.Date)
                .OrderBy(a => a.Task.TaskName)
                .ToListAsync();

            return assignments.Select(MapToDto);
        }

        public async Task<IEnumerable<AssignmentDto>> GetAssignmentsByStatusAsync(string status)
        {
            var assignments = await _context.Assignments
                .Include(a => a.Task)
                .Include(a => a.User)
                .Include(a => a.AssignedByUser)
                .Where(a => a.Status == status)
                .OrderByDescending(a => a.EventDate)
                .ToListAsync();

            return assignments.Select(MapToDto);
        }

        public async Task<AssignmentValidationResult> ValidateAssignmentAsync(int taskId, int userId, DateTime eventDate, bool isOverride)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            // Ensure eventDate is in UTC
            var eventDateUtc = eventDate.Kind == DateTimeKind.Utc 
                ? eventDate 
                : DateTime.SpecifyKind(eventDate, DateTimeKind.Utc);

            // Check if task exists
            var task = await _context.Tasks
                .Include(t => t.RequiredSkill)
                .FirstOrDefaultAsync(t => t.TaskId == taskId);

            if (task == null)
            {
                errors.Add("Task not found");
                return new AssignmentValidationResult(false, errors, warnings);
            }

            // Check if user exists
            var user = await _context.Users
                .Include(u => u.UserSkills)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                errors.Add("User not found");
                return new AssignmentValidationResult(false, errors, warnings);
            }

            // Check if user is active
            if (!user.IsActive)
            {
                errors.Add("User is not active");
            }

            // BUSINESS RULE 1: Qualification Check (BR-AS-002)
            if (task.IsRestricted && task.RequiredSkillId.HasValue)
            {
                var hasRequiredSkill = user.UserSkills.Any(us => us.SkillId == task.RequiredSkillId.Value);
                if (!hasRequiredSkill)
                {
                    errors.Add($"User does not have the required skill: {task.RequiredSkill?.SkillName}");
                }
            }

            // BUSINESS RULE 2: Conflict Detection - Same Day (BR-AS-003)
            var existingAssignment = await _context.Assignments
                .AnyAsync(a => a.UserId == userId && 
                              a.EventDate.Date == eventDateUtc.Date && 
                              a.Status != "Rejected" &&
                              a.Status != "Expired");

            if (existingAssignment)
            {
                errors.Add($"User already has an assignment on {eventDateUtc:yyyy-MM-dd}");
            }

            // BUSINESS RULE 3: Monthly Limit Check (BR-AS-004) - Warning only
            if (user.MonthlyLimit.HasValue)
            {
                var monthStart = new DateTime(eventDateUtc.Year, eventDateUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var monthlyCount = await _context.Assignments
                    .CountAsync(a => a.UserId == userId &&
                                    a.EventDate >= monthStart &&
                                    a.EventDate <= monthEnd &&
                                    (a.Status == "Accepted" || a.Status == "Confirmed" || a.Status == "Completed"));

                if (monthlyCount >= user.MonthlyLimit.Value)
                {
                    warnings.Add($"User has reached their monthly limit of {user.MonthlyLimit.Value} assignments");
                }
            }

            // BUSINESS RULE 4: Past Date Check
            if (eventDateUtc.Date < DateTime.UtcNow.Date)
            {
                errors.Add("Cannot assign tasks to past dates");
            }

            // Override allows bypassing errors (except task/user not found)
            var isValid = errors.Count == 0 || (isOverride && errors.All(e => e.Contains("not found") == false));

            return new AssignmentValidationResult(isValid, errors, warnings);
        }

        public async Task<bool> RevokeAssignmentAsync(int assignmentId, string reason)
        {
            _logger.LogInformation("Revoking assignment {AssignmentId} with reason: {Reason}", assignmentId, reason);

            var assignment = await _context.Assignments
                .Include(a => a.User)
                .Include(a => a.Task)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment == null)
            {
                _logger.LogWarning("Assignment {AssignmentId} not found", assignmentId);
                return false;
            }

            // Only allow revoking pending assignments
            if (assignment.Status != "Pending")
            {
                _logger.LogWarning("Cannot revoke assignment {AssignmentId} with status {Status}", assignmentId, assignment.Status);
                return false;
            }

            try
            {
                // Send email notification
                await _emailService.SendAssignmentRevokedNotificationAsync(
                    assignment.User.Email,
                    assignment.User.Name,
                    assignment.Task.TaskName,
                    assignment.EventDate,
                    reason);

                // Delete the assignment
                _context.Assignments.Remove(assignment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully revoked assignment {AssignmentId}", assignmentId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking assignment {AssignmentId}", assignmentId);
                return false;
            }
        }

        public async Task<bool> SendManualReminderAsync(int assignmentId)
        {
            _logger.LogInformation("Sending manual reminder for assignment {AssignmentId}", assignmentId);

            var assignment = await _context.Assignments
                .Include(a => a.User)
                .Include(a => a.Task)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment == null)
            {
                _logger.LogWarning("Assignment {AssignmentId} not found", assignmentId);
                return false;
            }

            try
            {
                var daysUntil = (assignment.EventDate.Date - DateTime.UtcNow.Date).Days;

                await _emailService.SendAssignmentReminderAsync(
                    assignment.User.Email,
                    assignment.User.Name,
                    assignment.Task.TaskName,
                    assignment.EventDate,
                    daysUntil);

                _logger.LogInformation("Successfully sent manual reminder for assignment {AssignmentId}", assignmentId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending manual reminder for assignment {AssignmentId}", assignmentId);
                return false;
            }
        }

        public async Task<int> AutoUpdatePastAssignmentStatusesAsync()
        {
            var todayUtc = DateTime.UtcNow.Date;

            var assignmentsToExpire = await _context.Assignments
                .Where(a => a.EventDate.Date < todayUtc && a.Status == "Pending")
                .ToListAsync();

            var assignmentsToComplete = await _context.Assignments
                .Where(a => a.EventDate.Date < todayUtc &&
                            (a.Status == "Accepted" || a.Status == "Confirmed"))
                .ToListAsync();

            foreach (var assignment in assignmentsToExpire)
            {
                assignment.Status = "Expired";
                assignment.UpdatedAt = DateTime.UtcNow;
            }

            foreach (var assignment in assignmentsToComplete)
            {
                assignment.Status = "Completed";
                assignment.UpdatedAt = DateTime.UtcNow;
            }

            var totalUpdated = assignmentsToExpire.Count + assignmentsToComplete.Count;

            if (totalUpdated > 0)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation(
                    "Auto-updated {ExpiredCount} assignments to Expired and {CompletedCount} assignments to Completed",
                    assignmentsToExpire.Count,
                    assignmentsToComplete.Count);
            }

            return totalUpdated;
        }

        private AssignmentDto MapToDto(Assignment assignment)
        {
            return new AssignmentDto(
                assignment.AssignmentId,
                assignment.TaskId,
                assignment.Task.TaskName,
                assignment.UserId,
                assignment.User.Name,
                assignment.EventDate,
                assignment.Status,
                assignment.RejectionReason,
                assignment.IsOverride,
                assignment.AssignedBy,
                assignment.AssignedByUser.Name,
                assignment.CreatedAt
            );
        }
    }
}
