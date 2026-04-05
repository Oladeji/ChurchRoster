using ChurchRoster.Application.DTOs.Assignments;

namespace ChurchRoster.Application.Interfaces
{
    public interface IAssignmentService
    {
        Task<IEnumerable<AssignmentDto>> GetAllAssignmentsAsync();
        Task<AssignmentDto?> GetAssignmentByIdAsync(int assignmentId);
        Task<AssignmentDto?> CreateAssignmentAsync(CreateAssignmentRequest request, int assignedByUserId);
        Task<AssignmentDto?> UpdateAssignmentStatusAsync(int assignmentId, UpdateAssignmentStatusRequest request);
        Task<bool> DeleteAssignmentAsync(int assignmentId);
        Task<IEnumerable<AssignmentDto>> GetAssignmentsByUserAsync(int userId);
        Task<IEnumerable<AssignmentDto>> GetAssignmentsByDateAsync(DateTime eventDate);
        Task<IEnumerable<AssignmentDto>> GetAssignmentsByStatusAsync(string status);
        Task<AssignmentValidationResult> ValidateAssignmentAsync(int taskId, int userId, DateTime eventDate, bool isOverride);
        Task<bool> RevokeAssignmentAsync(int assignmentId, string reason);
        Task<bool> SendManualReminderAsync(int assignmentId);
        Task<int> AutoUpdatePastAssignmentStatusesAsync();
    }
}
