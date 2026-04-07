using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ChurchRoster.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Build configuration
        var currentDirectory = Directory.GetCurrentDirectory();
        var apiBasePath = Path.Combine(currentDirectory, "backend", "ChurchRoster.Api");

        if (!File.Exists(Path.Combine(apiBasePath, "appsettings.json")))
        {
            apiBasePath = Path.GetFullPath(Path.Combine(currentDirectory, "..", "ChurchRoster.Api"));
        }

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(apiBasePath, "appsettings.json"), optional: false)
            .AddJsonFile(Path.Combine(apiBasePath, "appsettings.Development.json"), optional: true)
            .Build();

        // Prefer a simpler connection string for design-time EF operations/migrations
        var connectionString = configuration.GetConnectionString("DefaultConnection2")
            ?? configuration.GetConnectionString("DefaultConnection");

        // Create DbContextOptions
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new AppDbContext(optionsBuilder.Options, new TenantContext { TenantId = 1, TenantName = "Default Church" });
    }
}
