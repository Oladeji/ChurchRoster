using ChurchRoster.Application.Interfaces;
using ChurchRoster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ChurchRoster.Api.Endpoints.V1
{
    public static class DiagnosticEndpoints
    {
        public static void MapDiagnosticEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/diagnostics").WithTags("Diagnostics");

            group.MapGet("/email-config", GetEmailConfiguration)
                .WithName("GetEmailConfiguration")
                .Produces<object>(StatusCodes.Status200OK);

            group.MapPost("/test-email", TestEmail)
                .WithName("TestEmail")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError);

            group.MapGet("/database-health", TestDatabaseConnection)
                .WithName("TestDatabaseConnection")
                .Produces<object>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError);
        }

        private static IResult GetEmailConfiguration(
            IConfiguration configuration,
            ILogger<Program> logger)
        {
            logger.LogInformation("📋 Checking Email Configuration");

            var smtpHost = configuration["EmailSettings:SmtpServer"];
            var smtpPort = configuration["EmailSettings:SmtpPort"];
            var username = configuration["EmailSettings:Username"];
            var password = configuration["EmailSettings:Password"];
            var senderEmail = configuration["EmailSettings:SenderEmail"];
            var senderName = configuration["EmailSettings:SenderName"];
            var frontendUrl = configuration["App:FrontendUrl"];

            var config = new
            {
                SmtpHost = smtpHost ?? "NOT SET",
                SmtpPort = smtpPort ?? "NOT SET",
                Username = string.IsNullOrEmpty(username) ? "NOT SET" : "✓ SET (hidden)",
                Password = string.IsNullOrEmpty(password) ? "NOT SET" : "✓ SET (hidden)",
                SenderEmail = senderEmail ?? "NOT SET",
                SenderName = senderName ?? "NOT SET",
                FrontendUrl = frontendUrl ?? "NOT SET",
                IsConfigured = !string.IsNullOrEmpty(smtpHost) && 
                               !string.IsNullOrEmpty(username) && 
                               !string.IsNullOrEmpty(password)
            };

            logger.LogInformation("Email Configuration Status: {IsConfigured}", config.IsConfigured);

            return Results.Ok(config);
        }

        private static async Task<IResult> TestEmail(
            TestEmailRequest request,
            IEmailService emailService,
            ILogger<Program> logger)
        {
            logger.LogInformation("🧪 Testing email send to: {Email}", request.ToEmail);

            try
            {
                var result = await emailService.SendInvitationEmailAsync(
                    request.ToEmail,
                    request.ToName ?? "Test User",
                    "TEST-TOKEN-123456",
                    "System Administrator");

                if (result)
                {
                    logger.LogInformation("✅ Test email sent successfully");
                    return Results.Ok(new
                    {
                        success = true,
                        message = $"Test email sent successfully to {request.ToEmail}. Check the logs and your inbox."
                    });
                }
                else
                {
                    logger.LogError("❌ Test email failed to send");
                    return Results.Problem(
                        title: "Email Send Failed",
                        detail: "Failed to send test email. Check the application logs for details.",
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Exception while testing email");
                return Results.Problem(
                    title: "Email Test Exception",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        private static async Task<IResult> TestDatabaseConnection(
            AppDbContext dbContext,
            ILogger<Program> logger)
        {
            logger.LogInformation("🔍 Testing database connection...");

            try
            {
                var startTime = DateTime.UtcNow;

                // Test 1: Can we connect?
                var canConnect = await dbContext.Database.CanConnectAsync();
                var connectionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

                if (!canConnect)
                {
                    logger.LogError("❌ Cannot connect to database");
                    return Results.Problem(
                        title: "Database Connection Failed",
                        detail: "Unable to establish connection to the database.",
                        statusCode: StatusCodes.Status500InternalServerError);
                }

                logger.LogInformation("✅ Database connection successful ({ConnectionTime}ms)", connectionTime);

                // Test 2: Can we query?
                startTime = DateTime.UtcNow;
                var userCount = await dbContext.Users.CountAsync();
                var queryTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

                logger.LogInformation("✅ Database query successful ({QueryTime}ms)", queryTime);

                // Get connection info
                var connectionString = dbContext.Database.GetConnectionString();
                var host = connectionString?.Split(';').FirstOrDefault(s => s.StartsWith("Host="))?.Split('=')[1];
                var port = connectionString?.Split(';').FirstOrDefault(s => s.StartsWith("Port="))?.Split('=')[1];

                return Results.Ok(new
                {
                    success = true,
                    message = "Database connection is healthy",
                    connectionTimeMs = connectionTime,
                    queryTimeMs = queryTime,
                    userCount,
                    databaseInfo = new
                    {
                        host = host ?? "unknown",
                        port = port ?? "unknown",
                        pooling = connectionString?.Contains("Pooling=true") ?? false,
                        provider = "PostgreSQL (Npgsql)"
                    },
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Database connection test failed");
                return Results.Problem(
                    title: "Database Test Failed",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }

    public record TestEmailRequest(string ToEmail, string? ToName);
}
