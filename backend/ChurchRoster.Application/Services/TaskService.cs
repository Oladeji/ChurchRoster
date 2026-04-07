using ChurchRoster.Application.DTOs.Tasks;
using ChurchRoster.Application.Interfaces;
using ChurchRoster.Core.Entities;
using ChurchRoster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChurchRoster.Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;
        private readonly ITenantContext _tenantContext;

        public TaskService(AppDbContext context, ITenantContext tenantContext)
        {
            _context = context;
            _tenantContext = tenantContext;
        }

        public async Task<IEnumerable<TaskDto>> GetAllTasksAsync()
        {
            var tasks = await _context.Tasks
                .Include(t => t.RequiredSkill)
                .OrderBy(t => t.TaskName)
                .ToListAsync();

            return tasks.Select(MapToDto);
        }

        public async Task<TaskDto?> GetTaskByIdAsync(int taskId)
        {
            var task = await _context.Tasks
                .Include(t => t.RequiredSkill)
                .FirstOrDefaultAsync(t => t.TaskId == taskId);

            return task == null ? null : MapToDto(task);
        }

        public async Task<TaskDto?> CreateTaskAsync(CreateTaskRequest request)
        {
            if (!_tenantContext.TenantId.HasValue)
                return null;

            // Validate required skill exists if provided
            if (request.RequiredSkillId.HasValue)
            {
                var skillExists = await _context.Skills.AnyAsync(s => s.SkillId == request.RequiredSkillId.Value);
                if (!skillExists)
                    return null;
            }

            var task = new MinistryTask
            {
                TenantId = _tenantContext.TenantId.Value,
                TaskName = request.TaskName,
                Frequency = request.Frequency,
                DayRule = request.DayRule,
                RequiredSkillId = request.RequiredSkillId,
                IsRestricted = request.IsRestricted,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return await GetTaskByIdAsync(task.TaskId);
        }

        public async Task<TaskDto?> UpdateTaskAsync(int taskId, UpdateTaskRequest request)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.TaskId == taskId);
            if (task == null)
                return null;

            // Validate required skill exists if provided
            if (request.RequiredSkillId.HasValue)
            {
                var skillExists = await _context.Skills.AnyAsync(s => s.SkillId == request.RequiredSkillId.Value);
                if (!skillExists)
                    return null;
            }

            task.TaskName = request.TaskName;
            task.Frequency = request.Frequency;
            task.DayRule = request.DayRule;
            task.RequiredSkillId = request.RequiredSkillId;
            task.IsRestricted = request.IsRestricted;
            task.IsActive = request.IsActive;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetTaskByIdAsync(taskId);
        }

        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.TaskId == taskId);
            if (task == null)
                return false;

            // Soft delete
            task.IsActive = false;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByFrequencyAsync(string frequency)
        {
            var tasks = await _context.Tasks
                .Include(t => t.RequiredSkill)
                .Where(t => t.Frequency == frequency)
                .OrderBy(t => t.TaskName)
                .ToListAsync();

            return tasks.Select(MapToDto);
        }

        public async Task<IEnumerable<TaskDto>> GetRestrictedTasksAsync()
        {
            var tasks = await _context.Tasks
                .Include(t => t.RequiredSkill)
                .Where(t => t.IsRestricted)
                .OrderBy(t => t.TaskName)
                .ToListAsync();

            return tasks.Select(MapToDto);
        }

        public async Task<IEnumerable<TaskDto>> GetActiveTasksAsync()
        {
            var tasks = await _context.Tasks
                .Include(t => t.RequiredSkill)
                .Where(t => t.IsActive)
                .OrderBy(t => t.TaskName)
                .ToListAsync();

            return tasks.Select(MapToDto);
        }

        private TaskDto MapToDto(MinistryTask task)
        {
            return new TaskDto(
                task.TaskId,
                task.TaskName,
                task.Frequency,
                task.DayRule,
                task.RequiredSkillId,
                task.RequiredSkill?.SkillName,
                task.IsRestricted,
                task.IsActive,
                task.CreatedAt
            );
        }
    }
}
