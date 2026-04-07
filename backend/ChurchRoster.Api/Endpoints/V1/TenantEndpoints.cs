using ChurchRoster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChurchRoster.Api.Endpoints.V1;

public static class TenantEndpoints
{
    public static void MapTenantEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/tenants").WithTags("Tenants");

        group.MapGet("/", GetTenants)
            .WithName("GetTenants")
            .Produces(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetTenants(AppDbContext context)
    {
        var tenants = await context.Tenants
            .IgnoreQueryFilters()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .Select(t => new
            {
                tenantId = t.TenantId,
                name = t.Name,
                slug = t.Slug
            })
            .ToListAsync();

        return Results.Ok(tenants);
    }
}
