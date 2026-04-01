using Asp.Versioning;
using ChurchRoster.Api;
using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;
var builder = WebApplication.CreateBuilder(args);
// Add PORT support for Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddAPIServices(builder.Configuration);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
});
var app = builder.Build();
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
        options.WithTitle("My API Docs");
        options.WithTheme(ScalarTheme.Kepler);
    });
}
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


app.Run();


