using Asp.Versioning;
using ChurchRoster.Application.Interfaces;
using ChurchRoster.Application.Services;
using ChurchRoster.Infrastructure.Data;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ChurchRoster.Api
{
    public static  class APIServiceCollection
    {
        public static IServiceCollection AddAPIServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // Add Application Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<ISkillService, SkillService>();
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<IAssignmentService, AssignmentService>();

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
            //  var origins = configuration.GetSection(CorsConstants.CorsOrigins_PermittedClients).Get<string[]>();
            var origins = new string[] { 
            "localhost:3000","http://localhost:3000","https://localhost:3000",
            };
            services.AddCors(options =>
            {
                options.AddPolicy("CorsConstants.Cors_Policy", builder =>
                {
                    builder.AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithOrigins(origins)
                        .AllowCredentials();
                });
            });
            return services;
        }

    }
}
