using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PortfolioWeb.Core.Contracts.Repositories;
using PortfolioWeb.Infrastructure.Persistence;
using PortfolioWeb.Infrastructure.Repositories;

namespace PortfolioWeb.Infrastructure.Tests.DependencyInjection;

public class DependencyInjectionTest
{
    [Test]
    public void AddInfrastructure_ShouldRegisterRepositoriesAndDbContext()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PortfolioWebDatabase"] = "Host=localhost;Database=portfolio;Username=postgres;Password=postgres"
            })
            .Build();

        var result = services.AddInfrastructure(configuration);
        var authorRepositoryDescriptor = services.Single(service => service.ServiceType == typeof(IAuthorRepository));
        var projectRepositoryDescriptor = services.Single(service => service.ServiceType == typeof(IProjectRepository));
        var userRepositoryDescriptor = services.Single(service => service.ServiceType == typeof(IUserRepository));
        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<PortfolioWebDbContext>();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(services));
            Assert.That(authorRepositoryDescriptor.ImplementationType, Is.EqualTo(typeof(AuthorRepository)));
            Assert.That(authorRepositoryDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
            Assert.That(projectRepositoryDescriptor.ImplementationType, Is.EqualTo(typeof(ProjectRepository)));
            Assert.That(projectRepositoryDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
            Assert.That(userRepositoryDescriptor.ImplementationType, Is.EqualTo(typeof(UserRepository)));
            Assert.That(userRepositoryDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
            Assert.That(context.Database.ProviderName, Is.EqualTo("Npgsql.EntityFrameworkCore.PostgreSQL"));
        });
    }
}
