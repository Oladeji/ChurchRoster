using ChurchRoster.Application.DTOs.Skills;
using ChurchRoster.Application.Interfaces;

namespace ChurchRoster.Api.Endpoints.V1
{
    public static class SkillEndpoints
    {
        public static void MapSkillEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/skills").WithTags("Skills");

            group.MapGet("/", GetAllSkills)
                .WithName("GetAllSkills")
                .Produces<IEnumerable<SkillDto>>(StatusCodes.Status200OK);

            group.MapGet("/{id:int}", GetSkillById)
                .WithName("GetSkillById")
                .Produces<SkillDto>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPost("/", CreateSkill)
                .WithName("CreateSkill")
                .Produces<SkillDto>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status409Conflict);

            group.MapPut("/{id:int}", UpdateSkill)
                .WithName("UpdateSkill")
                .Produces<SkillDto>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status409Conflict);

            group.MapDelete("/{id:int}", DeleteSkill)
                .WithName("DeleteSkill")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPost("/assign", AssignSkillToUser)
                .WithName("AssignSkillToUser")
                .Produces<UserSkillDto>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status409Conflict);

            group.MapDelete("/assign/{userId:int}/{skillId:int}", RemoveSkillFromUser)
                .WithName("RemoveSkillFromUser")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/user/{userId:int}", GetUserSkills)
                .WithName("GetUserSkills")
                .Produces<IEnumerable<UserSkillDto>>(StatusCodes.Status200OK);

            group.MapGet("/{skillId:int}/users", GetSkillUsers)
                .WithName("GetSkillUsers")
                .Produces<IEnumerable<UserSkillDto>>(StatusCodes.Status200OK);
        }

        private static async Task<IResult> GetAllSkills(ISkillService skillService)
        {
            var skills = await skillService.GetAllSkillsAsync();
            return Results.Ok(skills);
        }

        private static async Task<IResult> GetSkillById(int id, ISkillService skillService)
        {
            var skill = await skillService.GetSkillByIdAsync(id);
            return skill == null ? Results.NotFound() : Results.Ok(skill);
        }

        private static async Task<IResult> CreateSkill(CreateSkillRequest request, ISkillService skillService)
        {
            if (string.IsNullOrWhiteSpace(request.SkillName))
            {
                return Results.BadRequest(new { message = "Skill name is required" });
            }

            var skill = await skillService.CreateSkillAsync(request);

            if (skill == null)
            {
                return Results.Conflict(new { message = "Skill name already exists" });
            }

            return Results.Created($"/api/skills/{skill.SkillId}", skill);
        }

        private static async Task<IResult> UpdateSkill(int id, UpdateSkillRequest request, ISkillService skillService)
        {
            if (string.IsNullOrWhiteSpace(request.SkillName))
            {
                return Results.BadRequest(new { message = "Skill name is required" });
            }

            var skill = await skillService.UpdateSkillAsync(id, request);

            if (skill == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(skill);
        }

        private static async Task<IResult> DeleteSkill(int id, ISkillService skillService)
        {
            var result = await skillService.DeleteSkillAsync(id);
            return result ? Results.NoContent() : Results.NotFound();
        }

        private static async Task<IResult> AssignSkillToUser(AssignSkillRequest request, ISkillService skillService)
        {
            var userSkill = await skillService.AssignSkillToUserAsync(request);

            if (userSkill == null)
            {
                return Results.Conflict(new { message = "User or skill not found, or skill already assigned" });
            }

            return Results.Created($"/api/skills/user/{request.UserId}", userSkill);
        }

        private static async Task<IResult> RemoveSkillFromUser(int userId, int skillId, ISkillService skillService)
        {
            var result = await skillService.RemoveSkillFromUserAsync(userId, skillId);
            return result ? Results.NoContent() : Results.NotFound();
        }

        private static async Task<IResult> GetUserSkills(int userId, ISkillService skillService)
        {
            var userSkills = await skillService.GetUserSkillsAsync(userId);
            return Results.Ok(userSkills);
        }

        private static async Task<IResult> GetSkillUsers(int skillId, ISkillService skillService)
        {
            var skillUsers = await skillService.GetSkillUsersAsync(skillId);
            return Results.Ok(skillUsers);
        }
    }
}
