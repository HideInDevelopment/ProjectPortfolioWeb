using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PortfolioWeb.Domain.Contract.Repositories;
using PortfolioWeb.Infrastructure.Persistence;
using PortfolioWeb.Infrastructure.Repositories;

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

        services.AddScoped<IAuthorRepository, AuthorRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();

        return services;
    }
}
