using Microsoft.Extensions.DependencyInjection;
using PortfolioWeb.Application.Contract.Services;
using PortfolioWeb.Application.Services;

namespace PortfolioWeb.Application.Tests.DependencyInjection;

public class DependencyInjectionTest
{
    [Test]
    public void AddApplication_ShouldRegisterApplicationServicesAsScoped()
    {
        var services = new ServiceCollection();

        var result = services.AddApplication();

        Assert.That(result, Is.SameAs(services));

        var authorServiceDescriptor = services.Single(service =>
            service.ServiceType == typeof(IAuthorService));
        var authServiceDescriptor = services.Single(service =>
            service.ServiceType == typeof(IAuthService));
        var projectServiceDescriptor = services.Single(service =>
            service.ServiceType == typeof(IProjectService));

        Assert.Multiple(() =>
        {
            Assert.That(authorServiceDescriptor.ImplementationType, Is.EqualTo(typeof(AuthorService)));
            Assert.That(authorServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
            Assert.That(authServiceDescriptor.ImplementationType, Is.EqualTo(typeof(AuthService)));
            Assert.That(authServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
            Assert.That(projectServiceDescriptor.ImplementationType, Is.EqualTo(typeof(ProjectService)));
            Assert.That(projectServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
        });
    }
}
