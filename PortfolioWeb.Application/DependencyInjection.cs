using Microsoft.Extensions.DependencyInjection;
using PortfolioWeb.Application.Contract.Services;
using PortfolioWeb.Application.Services;

namespace PortfolioWeb.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthorService, AuthorService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProjectService, ProjectService>();

        return services;
    }
}
