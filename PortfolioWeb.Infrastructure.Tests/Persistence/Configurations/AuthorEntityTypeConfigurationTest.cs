using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PortfolioWeb.Domain.Entities;
using PortfolioWeb.Infrastructure.Tests.Helpers;

namespace PortfolioWeb.Infrastructure.Tests.Persistence.Configurations;

public class AuthorEntityTypeConfigurationTest
{
    [Test]
    public void Configure_ShouldApplyExpectedAuthorMapping()
    {
        using var context = InMemoryDbContextFactory.CreateNpgsqlModelContext();
        var entityType = context.Model.FindEntityType(typeof(Author))!;
        var idProperty = entityType.FindProperty(nameof(Author.Id))!;
        var nameProperty = entityType.FindProperty(nameof(Author.Name))!;
        var navigation = entityType.FindNavigation(nameof(Author.Projects))!;
        var foreignKey = context.Model.FindEntityType(typeof(Project))!
            .GetForeignKeys()
            .Single(key => key.Properties.Single().Name == nameof(Project.AuthorId));

        Assert.Multiple(() =>
        {
            Assert.That(entityType.GetTableName(), Is.EqualTo("authors"));
            Assert.That(entityType.FindPrimaryKey()!.Properties.Single().Name, Is.EqualTo(nameof(Author.Id)));
            Assert.That(idProperty.ValueGenerated, Is.EqualTo(ValueGenerated.OnAdd));
            Assert.That(idProperty.GetDefaultValueSql(), Is.EqualTo("gen_random_uuid()"));
            Assert.That(nameProperty.GetMaxLength(), Is.EqualTo(200));
            Assert.That(nameProperty.IsNullable, Is.False);
            Assert.That(navigation.GetPropertyAccessMode(), Is.EqualTo(PropertyAccessMode.Field));
            Assert.That(foreignKey.PrincipalEntityType.ClrType, Is.EqualTo(typeof(Author)));
            Assert.That(foreignKey.DeclaringEntityType.ClrType, Is.EqualTo(typeof(Project)));
            Assert.That(foreignKey.Properties.Single().Name, Is.EqualTo(nameof(Project.AuthorId)));
            Assert.That(foreignKey.DeleteBehavior, Is.EqualTo(DeleteBehavior.Cascade));
        });
    }
}
