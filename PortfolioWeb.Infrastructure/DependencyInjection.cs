using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PortfolioWeb.Infrastructure.Persistence;

namespace PortfolioWeb.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PortfolioWebDatabase");

        services.AddDbContext<PortfolioWebDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }
}
