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

        private static async Task<IResult> Login(LoginRequest request, IAuthService authService, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("AuthEndpoints");

            try
            {
                logger.LogInformation("Login endpoint called for email: {Email}", request.Email);

                if (request.TenantId <= 0 || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {
                    logger.LogWarning("Login failed: Church, email or password is missing");
                    return Results.BadRequest(new { message = "Church, email and password are required" });
                }

                var response = await authService.LoginAsync(request);

                if (response == null)
                {
                    logger.LogWarning("Login failed for email: {Email}", request.Email);
                    return Results.Unauthorized();
                }

                logger.LogInformation("Login successful for email: {Email}", request.Email);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception in Login endpoint for email: {Email}", request.Email);
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: 500,
                    title: "Internal Server Error"
                );
            }
        }

        private static async Task<IResult> Register(RegisterRequest request, IAuthService authService)
        {
            if (request.TenantId <= 0 ||
                string.IsNullOrWhiteSpace(request.Name) || 
                string.IsNullOrWhiteSpace(request.Email) || 
                string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest(new { message = "Church, name, email, and password are required" });
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
