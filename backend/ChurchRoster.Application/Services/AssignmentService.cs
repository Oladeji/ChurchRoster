using ChurchRoster.Application.DTOs.Assignments;
using ChurchRoster.Application.Interfaces;
using ChurchRoster.Core.Entities;
using ChurchRoster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChurchRoster.Application.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly AppDbContext _context;

        public AssignmentService(AppDbContext context)
        {
            _context = context;
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
