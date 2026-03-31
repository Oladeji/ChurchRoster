using ChurchRoster.Application.DTOs.Tasks;
using ChurchRoster.Application.Interfaces;

namespace ChurchRoster.Api.Endpoints.V1
{
    public static class TaskEndpoints
    {
        public static void MapTaskEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/tasks").WithTags("Tasks");

            group.MapGet("/", GetAllTasks)
                .WithName("GetAllTasks")
                .Produces<IEnumerable<TaskDto>>(StatusCodes.Status200OK);

            group.MapGet("/{id:int}", GetTaskById)
                .WithName("GetTaskById")
                .Produces<TaskDto>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/frequency/{frequency}", GetTasksByFrequency)
                .WithName("GetTasksByFrequency")
                .Produces<IEnumerable<TaskDto>>(StatusCodes.Status200OK);

            group.MapGet("/restricted", GetRestrictedTasks)
                .WithName("GetRestrictedTasks")
                .Produces<IEnumerable<TaskDto>>(StatusCodes.Status200OK);

            group.MapGet("/active", GetActiveTasks)
                .WithName("GetActiveTasks")
                .Produces<IEnumerable<TaskDto>>(StatusCodes.Status200OK);

            group.MapPost("/", CreateTask)
                .WithName("CreateTask")
                .Produces<TaskDto>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapPut("/{id:int}", UpdateTask)
                .WithName("UpdateTask")
                .Produces<TaskDto>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapDelete("/{id:int}", DeleteTask)
                .WithName("DeleteTask")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);
        }

        private static async Task<IResult> GetAllTasks(ITaskService taskService)
        {
            var tasks = await taskService.GetAllTasksAsync();
            return Results.Ok(tasks);
        }

        private static async Task<IResult> GetTaskById(int id, ITaskService taskService)
        {
            var task = await taskService.GetTaskByIdAsync(id);
            return task == null ? Results.NotFound() : Results.Ok(task);
        }

        private static async Task<IResult> GetTasksByFrequency(string frequency, ITaskService taskService)
        {
            var tasks = await taskService.GetTasksByFrequencyAsync(frequency);
            return Results.Ok(tasks);
        }

        private static async Task<IResult> GetRestrictedTasks(ITaskService taskService)
        {
            var tasks = await taskService.GetRestrictedTasksAsync();
            return Results.Ok(tasks);
        }

        private static async Task<IResult> GetActiveTasks(ITaskService taskService)
        {
            var tasks = await taskService.GetActiveTasksAsync();
            return Results.Ok(tasks);
        }

        private static async Task<IResult> CreateTask(CreateTaskRequest request, ITaskService taskService)
        {
            if (string.IsNullOrWhiteSpace(request.TaskName) || 
                string.IsNullOrWhiteSpace(request.Frequency) || 
                string.IsNullOrWhiteSpace(request.DayRule))
            {
                return Results.BadRequest(new { message = "Task name, frequency, and day rule are required" });
            }

            var task = await taskService.CreateTaskAsync(request);

            if (task == null)
            {
                return Results.BadRequest(new { message = "Invalid required skill ID" });
            }

            return Results.Created($"/api/tasks/{task.TaskId}", task);
        }

        private static async Task<IResult> UpdateTask(int id, UpdateTaskRequest request, ITaskService taskService)
        {
            if (string.IsNullOrWhiteSpace(request.TaskName) || 
                string.IsNullOrWhiteSpace(request.Frequency) || 
                string.IsNullOrWhiteSpace(request.DayRule))
            {
                return Results.BadRequest(new { message = "Task name, frequency, and day rule are required" });
            }

            var task = await taskService.UpdateTaskAsync(id, request);

            if (task == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(task);
        }

        private static async Task<IResult> DeleteTask(int id, ITaskService taskService)
        {
            var result = await taskService.DeleteTaskAsync(id);
            return result ? Results.NoContent() : Results.NotFound();
        }
    }
}
