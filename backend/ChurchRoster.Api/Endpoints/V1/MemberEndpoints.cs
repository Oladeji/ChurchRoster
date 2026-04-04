using ChurchRoster.Application.DTOs.Members;
using ChurchRoster.Application.Interfaces;

namespace ChurchRoster.Api.Endpoints.V1
{
    public static class MemberEndpoints
    {
        public static void MapMemberEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/members").WithTags("Members");

            group.MapGet("/", GetAllMembers)
                .WithName("GetAllMembers")
                .Produces<IEnumerable<MemberDto>>(StatusCodes.Status200OK);

            group.MapGet("/{id:int}", GetMemberById)
                .WithName("GetMemberById")
                .Produces<MemberDto>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/role/{role}", GetMembersByRole)
                .WithName("GetMembersByRole")
                .Produces<IEnumerable<MemberDto>>(StatusCodes.Status200OK);

            group.MapGet("/active", GetActiveMembers)
                .WithName("GetActiveMembers")
                .Produces<IEnumerable<MemberDto>>(StatusCodes.Status200OK);

            group.MapPost("/", CreateMember)
                .WithName("CreateMember")
                .Produces<MemberDto>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status409Conflict);

            group.MapPut("/{id:int}", UpdateMember)
                .WithName("UpdateMember")
                .Produces<MemberDto>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapDelete("/{id:int}", DeleteMember)
                .WithName("DeleteMember")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPut("/{id:int}/password", UpdatePassword)
                .WithName("UpdatePassword")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/{id:int}/skills", GetMemberSkills)
                .WithName("GetMemberSkills")
                .Produces<IEnumerable<ChurchRoster.Application.DTOs.Skills.SkillDto>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPost("/{id:int}/skills", AssignSkillToMember)
                .WithName("AssignSkillToMember")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapDelete("/{id:int}/skills/{skillId:int}", RemoveSkillFromMember)
                .WithName("RemoveSkillFromMember")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/qualified/{taskId:int}", GetQualifiedMembers)
                .WithName("GetQualifiedMembers")
                .Produces<IEnumerable<MemberDto>>(StatusCodes.Status200OK);

            group.MapPost("/device-token", SaveDeviceToken)
                .WithName("SaveDeviceToken")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);
        }

        private static async Task<IResult> GetAllMembers(IMemberService memberService)
        {
            var members = await memberService.GetAllMembersAsync();
            return Results.Ok(members);
        }

        private static async Task<IResult> GetMemberById(int id, IMemberService memberService)
        {
            var member = await memberService.GetMemberByIdAsync(id);
            return member == null ? Results.NotFound() : Results.Ok(member);
        }

        private static async Task<IResult> GetMembersByRole(string role, IMemberService memberService)
        {
            var members = await memberService.GetMembersByRoleAsync(role);
            return Results.Ok(members);
        }

        private static async Task<IResult> GetActiveMembers(IMemberService memberService)
        {
            var members = await memberService.GetActiveMembersAsync();
            return Results.Ok(members);
        }

        private static async Task<IResult> CreateMember(CreateMemberRequest request, IMemberService memberService)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
            {
                return Results.BadRequest(new { message = "Name and email are required" });
            }

            var member = await memberService.CreateMemberAsync(request);

            if (member == null)
            {
                return Results.Conflict(new { message = "Email already exists or password does not meet requirements" });
            }

            return Results.Created($"/api/members/{member.UserId}", member);
        }

        private static async Task<IResult> UpdateMember(int id, UpdateMemberRequest request, IMemberService memberService)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.BadRequest(new { message = "Name is required" });
            }

            var member = await memberService.UpdateMemberAsync(id, request);

            if (member == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(member);
        }

        private static async Task<IResult> DeleteMember(int id, IMemberService memberService)
        {
            var result = await memberService.DeleteMemberAsync(id);
            return result ? Results.NoContent() : Results.NotFound();
        }

        private static async Task<IResult> UpdatePassword(int id, UpdatePasswordRequest request, IMemberService memberService)
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return Results.BadRequest(new { message = "Current password and new password are required" });
            }

            var result = await memberService.UpdatePasswordAsync(id, request);

            if (!result)
            {
                return Results.BadRequest(new { message = "Invalid current password or new password does not meet requirements" });
            }

            return Results.NoContent();
        }

        private static async Task<IResult> GetMemberSkills(int id, IMemberService memberService)
        {
            var member = await memberService.GetMemberByIdAsync(id);
            if (member == null)
            {
                return Results.NotFound(new { message = "Member not found" });
            }

            var skills = await memberService.GetMemberSkillsAsync(id);
            return Results.Ok(skills);
        }

        private static async Task<IResult> AssignSkillToMember(int id, AssignSkillRequest request, IMemberService memberService)
        {
            if (request.SkillId <= 0)
            {
                return Results.BadRequest(new { message = "Valid skill ID is required" });
            }

            var result = await memberService.AssignSkillToMemberAsync(id, request.SkillId);

            if (!result)
            {
                return Results.BadRequest(new { message = "Unable to assign skill. Member or skill not found, or skill already assigned." });
            }

            return Results.NoContent();
        }

        private static async Task<IResult> RemoveSkillFromMember(int id, int skillId, IMemberService memberService)
        {
            var result = await memberService.RemoveSkillFromMemberAsync(id, skillId);

            if (!result)
            {
                return Results.NotFound(new { message = "Skill assignment not found" });
            }

            return Results.NoContent();
        }

        private static async Task<IResult> GetQualifiedMembers(int taskId, IMemberService memberService)
        {
            var members = await memberService.GetQualifiedMembersForTaskAsync(taskId);
            return Results.Ok(members);
        }

        private static async Task<IResult> SaveDeviceToken(
            DeviceTokenRequest request,
            HttpContext httpContext,
            IMemberService memberService)
        {
            Console.WriteLine("[DEVICE_TOKEN] SaveDeviceToken endpoint called");
            Console.WriteLine($"[DEVICE_TOKEN] Device token received: {request.DeviceToken?.Substring(0, Math.Min(30, request.DeviceToken?.Length ?? 0))}...");

            if (string.IsNullOrWhiteSpace(request.DeviceToken))
            {
                Console.WriteLine("[DEVICE_TOKEN] ❌ Device token is empty");
                return Results.BadRequest(new { message = "Device token is required" });
            }

            // Log all claims for debugging
            Console.WriteLine("[DEVICE_TOKEN] JWT Claims in token:");
            foreach (var claim in httpContext.User.Claims)
            {
                Console.WriteLine($"[DEVICE_TOKEN]   - {claim.Type}: {claim.Value}");
            }

            // Get userId from JWT claims
            var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            Console.WriteLine($"[DEVICE_TOKEN] NameIdentifier claim: {(userIdClaim != null ? userIdClaim.Value : "NULL")}");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                Console.WriteLine("[DEVICE_TOKEN] ❌ Invalid user - NameIdentifier claim missing or invalid");
                return Results.BadRequest(new { message = "Invalid user" });
            }

            Console.WriteLine($"[DEVICE_TOKEN] ✅ UserId extracted from token: {userId}");
            Console.WriteLine($"[DEVICE_TOKEN] Updating device token for user {userId}...");

            var result = await memberService.UpdateDeviceTokenAsync(userId, request.DeviceToken);

            if (!result)
            {
                Console.WriteLine($"[DEVICE_TOKEN] ❌ User {userId} not found in database");
                return Results.NotFound(new { message = "User not found" });
            }

            Console.WriteLine($"[DEVICE_TOKEN] ✅ Device token saved successfully for user {userId}");
            return Results.Ok(new { message = "Device token saved successfully" });
        }

        public record AssignSkillRequest(int SkillId);
        public record DeviceTokenRequest(string DeviceToken);
    }
}
