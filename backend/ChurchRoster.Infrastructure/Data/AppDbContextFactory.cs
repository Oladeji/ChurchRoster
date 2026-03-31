using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ChurchRoster.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Build configuration
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../ChurchRoster.Api");

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(basePath, "appsettings.json"), optional: false)
            .AddJsonFile(Path.Combine(basePath, "appsettings.Development.json"), optional: true)
            .Build();

        // Get connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Create DbContextOptions
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
