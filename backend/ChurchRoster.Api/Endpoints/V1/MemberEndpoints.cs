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
    }
}
