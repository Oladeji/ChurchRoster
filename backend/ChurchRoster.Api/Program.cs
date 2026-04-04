using Asp.Versioning;
using ChurchRoster.Api;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configure detailed logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add detailed logging for specific namespaces
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.Information);
builder.Logging.AddFilter("ChurchRoster", LogLevel.Debug);

// Add PORT support for Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
//builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Log startup configuration
var logger = LoggerFactory.Create(config => config.AddConsole()).CreateLogger("Startup");
logger.LogInformation("=== Church Roster API Starting ===");
logger.LogInformation($"Port: {port}");
logger.LogInformation($"Environment: {builder.Environment.EnvironmentName}");

// Log connection string (masked)
var connString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connString))
{
    var maskedConnString = MaskConnectionString(connString);
    logger.LogInformation($"Connection String: {maskedConnString}");
}
else
{
    logger.LogError("WARNING: ConnectionStrings__DefaultConnection is not set!");
}

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddAPIServices(builder.Configuration);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

// Add global exception handler
app.Use(async (context, next) =>
{
    try
    {
        context.Request.Scheme = "https";
        await next();
    }
    catch (Exception ex)
    {
        var requestLogger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        requestLogger.LogError(ex, "Unhandled exception occurred. Path: {Path}, Method: {Method}", 
            context.Request.Path, context.Request.Method);

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Internal Server Error",
            message = ex.Message,
            path = context.Request.Path.ToString(),
            timestamp = DateTime.UtcNow
        });
    }
});

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Church Roster API");
        options.WithTheme(ScalarTheme.Kepler);
    });
}
app.UseCors("CorsConstants.Cors_Policy");

// Add Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Add request logging middleware
app.Use(async (context, next) =>
{
    var requestLogger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    requestLogger.LogInformation("==> Request: {Method} {Path}", context.Request.Method, context.Request.Path);

    await next();

    requestLogger.LogInformation("<== Response: {StatusCode} for {Method} {Path}", 
        context.Response.StatusCode, context.Request.Method, context.Request.Path);
});

var versionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .ReportApiVersions()
    .Build();

RouteGroupBuilder versionGroup =
    app.MapGroup(string.Empty)
        .WithApiVersionSet(versionSet)
        .MapToApiVersion(1);

versionGroup.RegisterEndpoints();
//app.UseHttpsRedirection();

app.Logger.LogInformation("=== Church Roster API Started Successfully ===");
app.Logger.LogInformation($"Listening on: http://0.0.0.0:{port}");

app.Run();

// Helper method to mask sensitive data in connection string
static string MaskConnectionString(string connectionString)
{
    if (string.IsNullOrEmpty(connectionString)) return "";

    var parts = connectionString.Split(';');
    var masked = new List<string>();

    foreach (var part in parts)
    {
        if (part.Contains("Password=", StringComparison.OrdinalIgnoreCase))
        {
            masked.Add("Password=***MASKED***");
        }
        else
        {
            masked.Add(part);
        }
    }

    return string.Join(";", masked);
}


