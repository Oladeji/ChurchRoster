using ChurchRoster.Application.DTOs.Invitations;
using ChurchRoster.Application.Interfaces;

namespace ChurchRoster.Api.Endpoints.V1
{
    public static class InvitationEndpoints
    {
        public static void MapInvitationEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/invitations").WithTags("Invitations");

            group.MapPost("/send", SendInvitation)
                .WithName("SendInvitation")
                .Produces<InvitationDto>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/verify/{token}", VerifyInvitation)
                .WithName("VerifyInvitation")
                .Produces<VerifyInvitationResponse>(StatusCodes.Status200OK);

            group.MapPost("/accept", AcceptInvitation)
                .WithName("AcceptInvitation")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/", GetAllInvitations)
                .WithName("GetAllInvitations")
                .Produces<IEnumerable<InvitationDto>>(StatusCodes.Status200OK);

            group.MapGet("/pending", GetPendingInvitations)
                .WithName("GetPendingInvitations")
                .Produces<IEnumerable<InvitationDto>>(StatusCodes.Status200OK);

            group.MapDelete("/{id:int}", CancelInvitation)
                .WithName("CancelInvitation")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);
        }

        private static async Task<IResult> SendInvitation(
            SendInvitationRequest request,
            IInvitationService invitationService,
            ILogger<Program> logger)
        {
            logger.LogInformation("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            logger.LogInformation("📧 API ENDPOINT: POST /api/invitations/send");
            logger.LogInformation("Email: {Email}, Name: {Name}, Role: {Role}", request.Email, request.Name, request.Role);

            // TODO: Get actual user ID from JWT claims
            int currentUserId = 1; // Hardcoded for now
            logger.LogInformation("Current User ID: {UserId}", currentUserId);

            var invitation = await invitationService.SendInvitationAsync(request, currentUserId);

            if (invitation == null)
            {
                logger.LogWarning("❌ Failed to send invitation for {Email}", request.Email);
                return Results.BadRequest(new
                {
                    message = "Failed to send invitation. User may already exist or have a pending invitation."
                });
            }

            logger.LogInformation("✅ Invitation created successfully: ID={InvitationId}", invitation.InvitationId);
            logger.LogInformation("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            return Results.Created($"/api/invitations/{invitation.InvitationId}", invitation);
        }

        private static async Task<IResult> VerifyInvitation(
            string token,
            IInvitationService invitationService)
        {
            var result = await invitationService.VerifyInvitationTokenAsync(token);
            return Results.Ok(result);
        }

        private static async Task<IResult> AcceptInvitation(
            AcceptInvitationRequest request,
            IInvitationService invitationService)
        {
            var success = await invitationService.AcceptInvitationAsync(request);

            if (!success)
            {
                return Results.BadRequest(new
                {
                    message = "Failed to accept invitation. Token may be invalid, expired, or already used."
                });
            }

            return Results.Ok(new { message = "Invitation accepted successfully. You can now log in." });
        }

        private static async Task<IResult> GetAllInvitations(IInvitationService invitationService)
        {
            var invitations = await invitationService.GetAllInvitationsAsync();
            return Results.Ok(invitations);
        }

        private static async Task<IResult> GetPendingInvitations(IInvitationService invitationService)
        {
            var invitations = await invitationService.GetPendingInvitationsAsync();
            return Results.Ok(invitations);
        }

        private static async Task<IResult> CancelInvitation(
            int id,
            IInvitationService invitationService)
        {
            var success = await invitationService.CancelInvitationAsync(id);

            if (!success)
            {
                return Results.NotFound(new { message = "Invitation not found or already used." });
            }

            return Results.NoContent();
        }
    }
}
