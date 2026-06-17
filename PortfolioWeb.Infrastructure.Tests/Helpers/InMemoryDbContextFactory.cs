using Microsoft.EntityFrameworkCore;
using PortfolioWeb.Infrastructure.Persistence;

namespace PortfolioWeb.Infrastructure.Tests.Helpers;

internal static class InMemoryDbContextFactory
{
    public static PortfolioWebDbContext Create()
    {
        var options = new DbContextOptionsBuilder<PortfolioWebDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new PortfolioWebDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }

    public static PortfolioWebDbContext CreateNpgsqlModelContext()
    {
        var options = new DbContextOptionsBuilder<PortfolioWebDbContext>()
            .UseNpgsql("Host=localhost;Database=portfolio_web_tests;Username=postgres;Password=postgres")
            .Options;

        return new PortfolioWebDbContext(options);
    }
}
