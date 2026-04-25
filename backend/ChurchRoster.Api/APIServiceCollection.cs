using Asp.Versioning;
using ChurchRoster.Api.BackgroundServices;
using ChurchRoster.Application.Interfaces;
using ChurchRoster.Application.Services;
using ChurchRoster.Core.Entities.Proposals;
using ChurchRoster.Infrastructure.Data;
using ChurchRoster.Infrastructure.Services.Proposals;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.Channels;

namespace ChurchRoster.Api
{
    public static  class APIServiceCollection
    {
        public static IServiceCollection AddAPIServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add DbContext with connection resilience
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptions =>
                    {
                        // Enable connection resilience
                        npgsqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorCodesToAdd: null);

                        // Set command timeout
                        npgsqlOptions.CommandTimeout(30);
                    });

                // Enable sensitive data logging in development
                if (configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

            // Add JWT Authentication
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT SecretKey is not configured in appsettings.json");
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddAuthorization();
            services.AddScoped<ITenantContext, TenantContext>();

            // Add Application Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<ISkillService, SkillService>();
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<IAssignmentService, AssignmentService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IInvitationService, InvitationService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IProposalService, ProposalService>();

            // Add Background Services
            services.AddHostedService<AssignmentReminderService>();
            services.AddHostedService<AssignmentStatusUpdateService>();

            // Roster Proposal Generation — algorithm selected via RosterGeneration:Algorithm in appsettings
            // Accepted values (case-insensitive): "Greedy" (default), "OrTools"
            services.Configure<RosterOptions>(configuration.GetSection("RosterGeneration"));

            var rosterAlgo = configuration.GetValue<string>("RosterGeneration:Algorithm") ?? "Greedy";
            Console.WriteLine($"[Roster] Generation algorithm: {rosterAlgo}");

            // Bounded channel — capacity 10, writer blocks if full (prevents memory pressure)
            services.AddSingleton(Channel.CreateBounded<int>(new BoundedChannelOptions(10)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,   // only ProposalGenerationJob reads
                SingleWriter = false   // multiple HTTP requests may write
            }));

            services.AddScoped<GreedyProposalService>();
            services.AddScoped<OrToolsProposalService>();
            services.AddScoped<ProposalAgentService>();
            services.AddHostedService<ProposalGenerationJob>();

            //services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method}{context.HttpContext.Request.Path}";
                    context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
                    var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
                    if (activity != null)
                    {
                        context.ProblemDetails.Extensions.TryAdd("traceId", activity.TraceId);
                        context.ProblemDetails.Extensions.TryAdd("spanId", activity.SpanId);
                    }

                };
            });
            services.AddApiVersioning(
                option =>
                {
                    option.ReportApiVersions = true;
                    option.AssumeDefaultVersionWhenUnspecified = true;
                    option.DefaultApiVersion = new ApiVersion(1);// ApiVersion.Default;// new ApiVersion(2, 0);
                    option.ApiVersionReader = ApiVersionReader.Combine(
                        new QueryStringApiVersionReader("api-version"),
                        new HeaderApiVersionReader("api-version"),
                        new MediaTypeApiVersionReader("version"),
                        new UrlSegmentApiVersionReader()
                        );


                }).AddApiExplorer(option =>
                {
                    //option.GroupNameFormat = "'v'V";
                    option.GroupNameFormat = "'v'VVV";
                    option.SubstituteApiVersionInUrl = true;

                });
            services.AddCorsFromOrigin(configuration);
            return services;
        }


        private static IServiceCollection AddCorsFromOrigin(this IServiceCollection services, IConfiguration configuration)
        {
            var origins = configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>()
                ?? new[]
                {
                    "http://localhost:3000",
                    "http://localhost:5173",
                    "https://localhost:3000"
                };

            services.AddCors(options =>
            {
                options.AddPolicy("CorsConstants.Cors_Policy", builder =>
                {
                    builder.AllowAnyMethod()
                        .AllowAnyHeader()
                        .SetIsOriginAllowed(origin => IsAllowedOrigin(origin, origins))
                        .AllowCredentials();
                });
            });
            return services;
        }

        private static bool IsAllowedOrigin(string? origin, IEnumerable<string> configuredOrigins)
        {

            if (origin == "http://localhost:3000") { 
            
            return true;
            }
            if (string.IsNullOrWhiteSpace(origin))
            {
                return false;
            }

            if (configuredOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
            {
                return false;
            }

            return uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)
                && uri.Host.StartsWith("church-roster", StringComparison.OrdinalIgnoreCase)
                && uri.Host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase);
        }

    }
}
