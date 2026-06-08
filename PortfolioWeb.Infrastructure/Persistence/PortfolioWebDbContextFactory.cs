using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PortfolioWeb.Infrastructure.Persistence;

public class PortfolioWebDbContextFactory : IDesignTimeDbContextFactory<PortfolioWebDbContext>
{
    public PortfolioWebDbContext CreateDbContext(string[] args)
    {
        var apiProjectPath = GetApiProjectPath();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("PortfolioWebDatabase");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'PortfolioWebDatabase' was not found in PortfolioWeb.Api appsettings files.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<PortfolioWebDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new PortfolioWebDbContext(optionsBuilder.Options);
    }

    private static string GetApiProjectPath()
    {
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (currentDirectory is not null)
        {
            var apiProjectPath = Path.Combine(currentDirectory.FullName, "PortfolioWeb.Api");

            if (Directory.Exists(apiProjectPath))
            {
                return apiProjectPath;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the PortfolioWeb.Api project directory.");
    }
}
