using Microsoft.EntityFrameworkCore;
using PortfolioWeb.Domain.Entities;
using PortfolioWeb.Infrastructure.Tests.Helpers;

namespace PortfolioWeb.Infrastructure.Tests.Persistence;

public class PortfolioWebDbContextTest
{
    [Test]
    public void PortfolioWebDbContext_ShouldExposeConfiguredDbSetsAndModelEntities()
    {
        using var context = InMemoryDbContextFactory.CreateNpgsqlModelContext();
        var authorEntityType = context.Model.FindEntityType(typeof(Author));
        var projectEntityType = context.Model.FindEntityType(typeof(Project));

        Assert.Multiple(() =>
        {
            Assert.That(context.Authors, Is.Not.Null);
            Assert.That(context.Projects, Is.Not.Null);
            Assert.That(authorEntityType, Is.Not.Null);
            Assert.That(projectEntityType, Is.Not.Null);
            Assert.That(authorEntityType!.GetTableName(), Is.EqualTo("authors"));
            Assert.That(projectEntityType!.GetTableName(), Is.EqualTo("projects"));
        });
    }
}
