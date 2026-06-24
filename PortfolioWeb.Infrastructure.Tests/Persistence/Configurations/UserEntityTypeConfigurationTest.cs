using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PortfolioWeb.Domain.Entities;
using PortfolioWeb.Infrastructure.Tests.Helpers;

namespace PortfolioWeb.Infrastructure.Tests.Persistence.Configurations;

public class UserEntityTypeConfigurationTest
{
    [Test]
    public void Configure_ShouldApplyExpectedUserMapping()
    {
        using var context = InMemoryDbContextFactory.CreateNpgsqlModelContext();
        var entityType = context.Model.FindEntityType(typeof(User))!;
        var idProperty = entityType.FindProperty(nameof(User.Id))!;
        var emailProperty = entityType.FindProperty(nameof(User.Email))!;
        var passwordHashProperty = entityType.FindProperty(nameof(User.PasswordHash))!;
        var createdDateProperty = entityType.FindProperty(nameof(User.CreatedDate))!;
        var roleProperty = entityType.FindProperty(nameof(User.Role))!;
        var isActiveProperty = entityType.FindProperty(nameof(User.IsActive))!;

        Assert.Multiple(() =>
        {
            Assert.That(entityType.GetTableName(), Is.EqualTo("users"));
            Assert.That(entityType.FindPrimaryKey()!.Properties.Single().Name, Is.EqualTo(nameof(User.Id)));
            Assert.That(idProperty.ValueGenerated, Is.EqualTo(ValueGenerated.OnAdd));
            Assert.That(idProperty.GetDefaultValueSql(), Is.EqualTo("gen_random_uuid()"));
            Assert.That(emailProperty.GetMaxLength(), Is.EqualTo(320));
            Assert.That(emailProperty.IsNullable, Is.False);
            Assert.That(entityType.GetIndexes().Single(index => index.Properties.Single().Name == nameof(User.Email)).IsUnique, Is.True);
            Assert.That(passwordHashProperty.IsNullable, Is.False);
            Assert.That(createdDateProperty.IsNullable, Is.False);
            Assert.That(roleProperty.IsNullable, Is.False);
            Assert.That(isActiveProperty.IsNullable, Is.False);
        });
    }
}
