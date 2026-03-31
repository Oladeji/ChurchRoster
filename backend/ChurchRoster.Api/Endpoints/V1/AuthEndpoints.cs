using ChurchRoster.Application.DTOs.Auth;
using ChurchRoster.Application.Interfaces;

namespace ChurchRoster.Api.Endpoints.V1
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/auth").WithTags("Authentication");

            group.MapPost("/login", Login)
                .WithName("Login")
                .Produces<AuthResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/register", Register)
                .WithName("Register")
                .Produces<AuthResponse>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status409Conflict);
        }

        private static async Task<IResult> Login(LoginRequest request, IAuthService authService)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest(new { message = "Email and password are required" });
            }

            var response = await authService.LoginAsync(request);

            if (response == null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(response);
        }

        private static async Task<IResult> Register(RegisterRequest request, IAuthService authService)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || 
                string.IsNullOrWhiteSpace(request.Email) || 
                string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest(new { message = "Name, email, and password are required" });
            }

            if (string.IsNullOrWhiteSpace(request.Phone))
            {
                return Results.BadRequest(new { message = "Phone number is required" });
            }

            var response = await authService.RegisterAsync(request);

            if (response == null)
            {
                return Results.Conflict(new { message = "Email already exists or password does not meet requirements (min 8 chars, uppercase, lowercase, number)" });
            }

            return Results.Created($"/api/users/{response.UserId}", response);
        }
    }
}
