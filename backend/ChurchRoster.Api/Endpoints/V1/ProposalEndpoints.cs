using ChurchRoster.Application.DTOs.Proposals;
using ChurchRoster.Application.Interfaces;
using ChurchRoster.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChurchRoster.Api.Endpoints.V1;

public static class ProposalEndpoints
{
    public static void MapProposalEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/api/v1/proposals")
            .WithTags("Proposals")
            .RequireAuthorization();

        // ── Queries ──────────────────────────────────────────────────────────

        group.MapGet("/", GetProposalList)
            .WithName("GetProposalList")
            .Produces<IEnumerable<ProposalSummaryDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:int}", GetProposalById)
            .WithName("GetProposalById")
            .Produces<ProposalDetailDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        // ── Commands ─────────────────────────────────────────────────────────

        group.MapPost("/", GenerateProposal)
            .WithName("GenerateProposal")
            .Produces<GenerateProposalResult>(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPatch("/{id:int}/items/{itemId:int}", UpdateProposalItem)
            .WithName("UpdateProposalItem")
            .Produces<ProposalItemDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/items", AddProposalItem)
            .WithName("AddProposalItem")
            .Produces<ProposalItemDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}/items/{itemId:int}", DeleteProposalItem)
            .WithName("DeleteProposalItem")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/publish", PublishProposal)
            .WithName("PublishProposal")
            .Produces<PublishProposalResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/archive", ArchiveProposal)
            .WithName("ArchiveProposal")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/retry", RetryProposal)
            .WithName("RetryProposal")
            .Produces<GenerateProposalResult>(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/pdf", GetProposalDraftPdf)
            .WithName("GetProposalDraftPdf")
            .Produces(StatusCodes.Status200OK, contentType: "application/pdf")
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);
    }

    // ── Handlers ─────────────────────────────────────────────────────────────

    private static async Task<IResult> GetProposalList(
        HttpContext httpContext,
        ITenantContext tenantContext,
        IProposalService proposalService)
    {
        if (!TryResolveTenant(httpContext, tenantContext))
            return Results.BadRequest(new { message = "Tenant context is required." });

        if (!IsAdmin(httpContext))
            return Results.Forbid();

        var proposals = await proposalService.GetProposalListAsync();
        return Results.Ok(proposals);
    }

    private static async Task<IResult> GetProposalById(
        int id,
        HttpContext httpContext,
        ITenantContext tenantContext,
        IProposalService proposalService)
    {
        if (!TryResolveTenant(httpContext, tenantContext))
            return Results.BadRequest(new { message = "Tenant context is required." });

        if (!IsAdmin(httpContext))
            return Results.Forbid();

        var proposal = await proposalService.GetProposalByIdAsync(id);
        return proposal is null ? Results.NotFound() : Results.Ok(proposal);
    }

    private static async Task<IResult> GenerateProposal(
        [FromBody] GenerateProposalRequest request,
        HttpContext httpContext,
        ITenantContext tenantContext,
        IProposalService proposalService)
    {
        if (!TryResolveTenant(httpContext, tenantContext))
            return Results.BadRequest(new { message = "Tenant context is required." });

        if (!IsAdmin(httpContext))
            return Results.Forbid();

        var userId = GetUserId(httpContext);
        if (userId is null)
            return Results.BadRequest(new { message = "Could not resolve user identity." });

        var result = await proposalService.GenerateProposalAsync(request, userId.Value);

        if (result is null)
            return Results.Conflict(new
            {
                message = "A proposal is already being generated for this tenant. Please wait for it to complete."
            });

        return Results.Accepted($"/api/v1/proposals/{result.ProposalId}", result);
    }

    private static async Task<IResult> UpdateProposalItem(
        int id,
        int itemId,
        [FromBody] UpdateProposalItemRequest request,
        HttpContext httpContext,
        ITenantContext tenantContext,
        IProposalService proposalService)
    {
        if (!TryResolveTenant(httpContext, tenantContext))
            return Results.BadRequest(new { message = "Tenant context is required." });

        if (!IsAdmin(httpContext))
            return Results.Forbid();

        var result = await proposalService.UpdateProposalItemAsync(itemId, request);

        if (result is null)
            return Results.Conflict(new
            {
                message = "Item not found or proposal is not in Draft status."
            });

        return Results.Ok(result);
    }

    private static async Task<IResult> AddProposalItem(
        int id,
        [FromBody] AddProposalItemRequest request,
        HttpContext httpContext,
        ITenantContext tenantContext,
        IProposalService proposalService)
    {
        if (!TryResolveTenant(httpContext, tenantContext))
            return Results.BadRequest(new { message = "Tenant context is required." });

        if (!IsAdmin(httpContext))
            return Results.Forbid();

        var result = await proposalService.AddProposalItemAsync(id, request);

        if (result is null)
            return Results.BadRequest(new
            {
                message = "Proposal not found, not in Draft status, or task/member not found."
            });

        return Results.Created($"/api/v1/proposals/{id}/items/{result.ItemId}", result);
    }

    private static async Task<IResult> DeleteProposalItem(
        int id,
        int itemId,
        HttpContext httpContext,
        ITenantContext tenantContext,
        IProposalService proposalService)
    {
        if (!TryResolveTenant(httpContext, tenantContext))
            return Results.BadRequest(new { message = "Tenant context is required." });

        if (!IsAdmin(httpContext))
            return Results.Forbid();

        var deleted = await proposalService.DeleteProposalItemAsync(itemId);

        if (!deleted)
            return Results.Conflict(new
            {
                message = "Item not found or proposal is not in Draft status."
            });

        return Results.NoContent();
    }

    private static async Task<IResult> PublishProposal(
        int id,
        HttpContext httpContext,
        ITenantContext tenantContext,
        IProposalService proposalService)
    {
        if (!TryResolveTenant(httpContext, tenantContext))
            return Results.BadRequest(new { message = "Tenant context is required." });

        if (!IsAdmin(httpContext))
            return Results.Forbid();

        var result = await proposalService.PublishProposalAsync(id);

        if (result is null)
            return Results.Conflict(new
            {
                message = "Proposal not found or is not in Draft status."
            });

        return Results.Ok(result);
    }

    private static async Task<IResult> ArchiveProposal(
        int id,
        HttpContext httpContext,
        ITenantContext tenantContext,
        IProposalService proposalService)
    {
        if (!TryResolveTenant(httpContext, tenantContext))
            return Results.BadRequest(new { message = "Tenant context is required." });

        if (!IsAdmin(httpContext))
            return Results.Forbid();

        var archived = await proposalService.ArchiveProposalAsync(id);

        if (!archived)
            return Results.Conflict(new
            {
                message = "Proposal not found or cannot be archived while Processing."
            });

        return Results.NoContent();
    }

    private static async Task<IResult> GetProposalDraftPdf(
        int id,
        HttpContext httpContext,
        ITenantContext tenantContext,
        IProposalService proposalService,
        IReportService reportService)
    {
        if (!TryResolveTenant(httpContext, tenantContext))
            return Results.BadRequest(new { message = "Tenant context is required." });

        if (!IsAdmin(httpContext))
            return Results.Forbid();

        // Verify proposal exists and belongs to this tenant
        var proposal = await proposalService.GetProposalByIdAsync(id);
        if (proposal is null)
            return Results.NotFound();

        try
        {
            var pdfBytes = await reportService.GenerateProposalDraftPdfAsync(id);
            return Results.File(pdfBytes, "application/pdf", $"Proposal_Draft_{id}_{proposal.Name}.pdf");
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to generate draft PDF: {ex.Message}");
        }
    }

    private static async Task<IResult> RetryProposal(
        int id,
        HttpContext httpContext,
        ITenantContext tenantContext,
        IProposalService proposalService)
    {
        if (!TryResolveTenant(httpContext, tenantContext))
            return Results.BadRequest(new { message = "Tenant context is required." });

        if (!IsAdmin(httpContext))
            return Results.Forbid();

        var userId = GetUserId(httpContext);
        if (userId is null)
            return Results.BadRequest(new { message = "Could not resolve user identity." });

        var result = await proposalService.RetryProposalAsync(id, userId.Value);

        if (result is null)
            return Results.NotFound(new { message = "Proposal not found." });

        return Results.Accepted($"/api/v1/proposals/{result.ProposalId}", result);
    }

    // ── Shared helpers (same pattern as ReportEndpoints) ─────────────────────

    private static bool TryResolveTenant(HttpContext httpContext, ITenantContext tenantContext)
    {
        if (tenantContext.TenantId.HasValue)
            return true;

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

    private static bool IsAdmin(HttpContext httpContext) =>
        httpContext.User.FindFirst(ClaimTypes.Role)?.Value == "Admin"
        || httpContext.User.FindFirst("role")?.Value == "Admin";

    private static int? GetUserId(HttpContext httpContext)
    {
        var value = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.User.FindFirst("sub")?.Value
            ?? httpContext.User.FindFirst("userId")?.Value;

        return int.TryParse(value, out var id) ? id : null;
    }
}
