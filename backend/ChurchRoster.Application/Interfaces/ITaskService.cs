using ChurchRoster.Application.DTOs.Tasks;

namespace ChurchRoster.Application.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskDto>> GetAllTasksAsync();
        Task<TaskDto?> GetTaskByIdAsync(int taskId);
        Task<TaskDto?> CreateTaskAsync(CreateTaskRequest request);
        Task<TaskDto?> UpdateTaskAsync(int taskId, UpdateTaskRequest request);
        Task<bool> DeleteTaskAsync(int taskId);
        Task<IEnumerable<TaskDto>> GetTasksByFrequencyAsync(string frequency);
        Task<IEnumerable<TaskDto>> GetRestrictedTasksAsync();
        Task<IEnumerable<TaskDto>> GetActiveTasksAsync();
    }
}
