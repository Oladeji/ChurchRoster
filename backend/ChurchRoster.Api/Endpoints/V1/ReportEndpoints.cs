using ChurchRoster.Application.Interfaces;
using ChurchRoster.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChurchRoster.Api.Endpoints.V1;

public static class ReportEndpoints
{
    public static void MapReportEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/reports").WithTags("Reports");

        group.MapGet("/monthly-roster", GetMonthlyRoster)
            .WithName("GetMonthlyRoster")
            .Produces(StatusCodes.Status200OK, contentType: "application/pdf")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapGet("/member-schedule", GetMemberSchedule)
            .WithName("GetMemberSchedule")
            .Produces(StatusCodes.Status200OK, contentType: "application/pdf")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapGet("/task-assignments", GetTaskAssignments)
            .WithName("GetTaskAssignments")
            .Produces(StatusCodes.Status200OK, contentType: "application/pdf")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> GetMonthlyRoster(
        [FromQuery] int year,
        [FromQuery] int month,
        HttpContext httpContext,
        ITenantContext tenantContext,
        IReportService reportService)
    {
        try
        {
            if (!TryResolveTenant(httpContext, tenantContext))
            {
                return Results.BadRequest(new { message = "Tenant is required to generate reports" });
            }

            // Validate inputs
            if (year < 2020 || year > 2100)
            {
                return Results.BadRequest(new { message = "Invalid year" });
            }

            if (month < 1 || month > 12)
            {
                return Results.BadRequest(new { message = "Invalid month. Must be between 1 and 12" });
            }

            var pdfBytes = await reportService.GenerateMonthlyRosterAsync(year, month);
            return Results.File(pdfBytes, "application/pdf", $"Monthly_Roster_{year}_{month:D2}.pdf");
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to generate report: {ex.Message}");
        }
    }

    private static async Task<IResult> GetMemberSchedule(
        [FromQuery] int? userId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        HttpContext httpContext,
        ITenantContext tenantContext,
        IReportService reportService)
    {
        try
        {
            if (!TryResolveTenant(httpContext, tenantContext))
            {
                return Results.BadRequest(new { message = "Tenant is required to generate reports" });
            }

            // Default date range if not provided
            var start = startDate ?? DateTime.Today;
            var end = endDate ?? DateTime.Today.AddMonths(1);

            // If userId not provided, use current user (from JWT claims)
            int targetUserId;
            if (userId.HasValue)
            {
                targetUserId = userId.Value;
            }
            else
            {
                // Get userId from JWT claims
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out targetUserId))
                {
                    return Results.BadRequest(new { message = "User ID is required" });
                }
            }

            var pdfBytes = await reportService.GenerateMemberScheduleAsync(targetUserId, start, end);
            return Results.File(pdfBytes, "application/pdf", $"Member_Schedule_{targetUserId}_{start:yyyyMMdd}.pdf");
        }
        catch (ArgumentException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to generate report: {ex.Message}");
        }
    }

    private static async Task<IResult> GetTaskAssignments(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        HttpContext httpContext,
        ITenantContext tenantContext,
        IReportService reportService)
    {
        try
        {
            if (!TryResolveTenant(httpContext, tenantContext))
            {
                return Results.BadRequest(new { message = "Tenant is required to generate reports" });
            }

            // Default date range if not provided
            var start = startDate ?? DateTime.Today;
            var end = endDate ?? DateTime.Today.AddMonths(1);

            if (start > end)
            {
                return Results.BadRequest(new { message = "Start date must be before end date" });
            }

            var pdfBytes = await reportService.GenerateTaskAssignmentReportAsync(start, end);
            return Results.File(pdfBytes, "application/pdf", $"Task_Assignments_{start:yyyyMMdd}.pdf");
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to generate report: {ex.Message}");
        }
    }

    private static bool TryResolveTenant(HttpContext httpContext, ITenantContext tenantContext)
    {
        if (tenantContext.TenantId.HasValue)
        {
            return true;
        }

        var tenantClaim = httpContext.User.FindFirst("tenant_id")?.Value
            ?? httpContext.User.FindFirst("TenantId")?.Value;

        if (int.TryParse(tenantClaim, out var claimTenantId))
        {
            tenantContext.TenantId = claimTenantId;
            return true;
        }

        if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader) &&
            int.TryParse(tenantHeader.FirstOrDefault(), out var headerTenantId))
        {
            tenantContext.TenantId = headerTenantId;
            return true;
        }

        if (httpContext.Request.Query.TryGetValue("tenantId", out var tenantQuery) &&
            int.TryParse(tenantQuery.FirstOrDefault(), out var queryTenantId))
        {
            tenantContext.TenantId = queryTenantId;
            return true;
        }

        return false;
    }
}
