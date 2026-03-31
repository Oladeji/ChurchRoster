using ChurchRoster.Application.DTOs.Assignments;
using ChurchRoster.Application.Interfaces;

namespace ChurchRoster.Api.Endpoints.V1
{
    public static class AssignmentEndpoints
    {
        public static void MapAssignmentEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/assignments").WithTags("Assignments");

            group.MapGet("/", GetAllAssignments)
                .WithName("GetAllAssignments")
                .Produces<IEnumerable<AssignmentDto>>(StatusCodes.Status200OK);

            group.MapGet("/{id:int}", GetAssignmentById)
                .WithName("GetAssignmentById")
                .Produces<AssignmentDto>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/user/{userId:int}", GetAssignmentsByUser)
                .WithName("GetAssignmentsByUser")
                .Produces<IEnumerable<AssignmentDto>>(StatusCodes.Status200OK);

            group.MapGet("/date/{eventDate:datetime}", GetAssignmentsByDate)
                .WithName("GetAssignmentsByDate")
                .Produces<IEnumerable<AssignmentDto>>(StatusCodes.Status200OK);

            group.MapGet("/status/{status}", GetAssignmentsByStatus)
                .WithName("GetAssignmentsByStatus")
                .Produces<IEnumerable<AssignmentDto>>(StatusCodes.Status200OK);

            group.MapPost("/", CreateAssignment)
                .WithName("CreateAssignment")
                .Produces<AssignmentDto>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapPost("/validate", ValidateAssignment)
                .WithName("ValidateAssignment")
                .Produces<AssignmentValidationResult>(StatusCodes.Status200OK);

            group.MapPut("/{id:int}/status", UpdateAssignmentStatus)
                .WithName("UpdateAssignmentStatus")
                .Produces<AssignmentDto>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapDelete("/{id:int}", DeleteAssignment)
                .WithName("DeleteAssignment")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);
        }

        private static async Task<IResult> GetAllAssignments(IAssignmentService assignmentService)
        {
            var assignments = await assignmentService.GetAllAssignmentsAsync();
            return Results.Ok(assignments);
        }

        private static async Task<IResult> GetAssignmentById(int id, IAssignmentService assignmentService)
        {
            var assignment = await assignmentService.GetAssignmentByIdAsync(id);
            return assignment == null ? Results.NotFound() : Results.Ok(assignment);
        }

        private static async Task<IResult> GetAssignmentsByUser(int userId, IAssignmentService assignmentService)
        {
            var assignments = await assignmentService.GetAssignmentsByUserAsync(userId);
            return Results.Ok(assignments);
        }

        private static async Task<IResult> GetAssignmentsByDate(DateTime eventDate, IAssignmentService assignmentService)
        {
            var assignments = await assignmentService.GetAssignmentsByDateAsync(eventDate);
            return Results.Ok(assignments);
        }

        private static async Task<IResult> GetAssignmentsByStatus(string status, IAssignmentService assignmentService)
        {
            var assignments = await assignmentService.GetAssignmentsByStatusAsync(status);
            return Results.Ok(assignments);
        }

        private static async Task<IResult> CreateAssignment(CreateAssignmentRequest request, IAssignmentService assignmentService)
        {
            // For now, assume the assigned by user ID is 1 (admin)
            // TODO: Get this from the authenticated user's claims
            int assignedByUserId = 1;

            // First validate
            var validation = await assignmentService.ValidateAssignmentAsync(
                request.TaskId, 
                request.UserId, 
                request.EventDate, 
                request.IsOverride);

            if (!validation.IsValid && !request.IsOverride)
            {
                return Results.BadRequest(new 
                { 
                    message = "Assignment validation failed",
                    errors = validation.Errors,
                    warnings = validation.Warnings
                });
            }

            var assignment = await assignmentService.CreateAssignmentAsync(request, assignedByUserId);

            if (assignment == null)
            {
                return Results.BadRequest(new { message = "Failed to create assignment" });
            }

            // Return with warnings if any
            if (validation.Warnings.Any())
            {
                return Results.Created($"/api/assignments/{assignment.AssignmentId}", new
                {
                    assignment,
                    warnings = validation.Warnings
                });
            }

            return Results.Created($"/api/assignments/{assignment.AssignmentId}", assignment);
        }

        private static async Task<IResult> ValidateAssignment(CreateAssignmentRequest request, IAssignmentService assignmentService)
        {
            var validation = await assignmentService.ValidateAssignmentAsync(
                request.TaskId,
                request.UserId,
                request.EventDate,
                request.IsOverride);

            return Results.Ok(validation);
        }

        private static async Task<IResult> UpdateAssignmentStatus(int id, UpdateAssignmentStatusRequest request, IAssignmentService assignmentService)
        {
            if (string.IsNullOrWhiteSpace(request.Status))
            {
                return Results.BadRequest(new { message = "Status is required" });
            }

            // Validate status values
            var validStatuses = new[] { "Pending", "Accepted", "Rejected", "Confirmed", "Completed", "Expired" };
            if (!validStatuses.Contains(request.Status))
            {
                return Results.BadRequest(new { message = $"Invalid status. Must be one of: {string.Join(", ", validStatuses)}" });
            }

            var assignment = await assignmentService.UpdateAssignmentStatusAsync(id, request);

            if (assignment == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(assignment);
        }

        private static async Task<IResult> DeleteAssignment(int id, IAssignmentService assignmentService)
        {
            var result = await assignmentService.DeleteAssignmentAsync(id);
            return result ? Results.NoContent() : Results.NotFound();
        }
    }
}
