using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PortfolioWeb.Domain.Entities;
using PortfolioWeb.Infrastructure.Tests.Helpers;

namespace PortfolioWeb.Infrastructure.Tests.Persistence.Configurations;

public class ProjectEntityTypeConfigurationTest
{
    [Test]
    public void Configure_ShouldApplyExpectedProjectMapping()
    {
        using var context = InMemoryDbContextFactory.CreateNpgsqlModelContext();
        var entityType = context.Model.FindEntityType(typeof(Project))!;
        var idProperty = entityType.FindProperty(nameof(Project.Id))!;
        var titleProperty = entityType.FindProperty(nameof(Project.Title))!;
        var descriptionProperty = entityType.FindProperty(nameof(Project.Description))!;
        var releaseDateProperty = entityType.FindProperty(nameof(Project.ReleaseDate))!;
        var versionProperty = entityType.FindProperty(nameof(Project.Version))!;
        var authorIdProperty = entityType.FindProperty(nameof(Project.AuthorId))!;
        var isInDevelopmentProperty = entityType.FindProperty(nameof(Project.IsInDevelopment))!;

        Assert.Multiple(() =>
        {
            Assert.That(entityType.GetTableName(), Is.EqualTo("projects"));
            Assert.That(entityType.FindPrimaryKey()!.Properties.Single().Name, Is.EqualTo(nameof(Project.Id)));
            Assert.That(idProperty.ValueGenerated, Is.EqualTo(ValueGenerated.OnAdd));
            Assert.That(idProperty.GetDefaultValueSql(), Is.EqualTo("gen_random_uuid()"));
            Assert.That(titleProperty.GetMaxLength(), Is.EqualTo(200));
            Assert.That(titleProperty.IsNullable, Is.False);
            Assert.That(descriptionProperty.GetMaxLength(), Is.EqualTo(2000));
            Assert.That(descriptionProperty.IsNullable, Is.False);
            Assert.That(releaseDateProperty.IsNullable, Is.False);
            Assert.That(versionProperty.IsNullable, Is.False);
            Assert.That(authorIdProperty.IsNullable, Is.False);
            Assert.That(isInDevelopmentProperty.IsNullable, Is.False);
        });
    }
}
