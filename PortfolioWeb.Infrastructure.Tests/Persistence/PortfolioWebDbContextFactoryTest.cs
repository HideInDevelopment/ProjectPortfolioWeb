using Microsoft.EntityFrameworkCore;
using PortfolioWeb.Infrastructure.Persistence;

namespace PortfolioWeb.Infrastructure.Tests.Persistence;

public class PortfolioWebDbContextFactoryTest
{
    [Test]
    public void CreateDbContext_ShouldReturnNpgsqlConfiguredContext_WhenApiProjectIsReachable()
    {
        var originalCurrentDirectory = Directory.GetCurrentDirectory();
        var solutionRootPath = Path.GetFullPath(Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "..",
            "..",
            "..",
            ".."));

        try
        {
            Directory.SetCurrentDirectory(solutionRootPath);
            var factory = new PortfolioWebDbContextFactory();

            using var context = factory.CreateDbContext([]);

            Assert.Multiple(() =>
            {
                Assert.That(context, Is.Not.Null);
                Assert.That(context.Database.ProviderName, Is.EqualTo("Npgsql.EntityFrameworkCore.PostgreSQL"));
                Assert.That(
                    context.Database.GetConnectionString(),
                    Is.EqualTo("Host=localhost;Port=5432;Database=portfolio_web_dev;Username=postgres;Password=postgres"));
            });
        }
        finally
        {
            Directory.SetCurrentDirectory(originalCurrentDirectory);
        }
    }
}
