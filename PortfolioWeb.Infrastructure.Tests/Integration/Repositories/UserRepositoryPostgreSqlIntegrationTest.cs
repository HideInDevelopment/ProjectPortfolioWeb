using Microsoft.EntityFrameworkCore;
using PortfolioWeb.Core.Contracts.Exceptions;
using PortfolioWeb.Domain.Entities;
using PortfolioWeb.Infrastructure.Repositories;
using PortfolioWeb.Infrastructure.Tests.Helpers;

namespace PortfolioWeb.Infrastructure.Tests.Integration.Repositories;

[NonParallelizable]
public class UserRepositoryPostgreSqlIntegrationTest
{
    private const string DatabaseName = "portfolio_web_user_integration_tests";

    [SetUp]
    public async Task SetUp()
    {
        if (!await PostgreSqlDbContextFactory.IsServerAvailableAsync())
        {
            Assert.Ignore("PostgreSQL is not available on localhost:5432. Start the database container to run integration tests.");
        }

        await PostgreSqlDbContextFactory.ResetDatabaseAsync(DatabaseName);
    }

    [Test]
    public async Task Create_ShouldPersistUserAndAuthorInPostgreSql()
    {
        await using var context = PostgreSqlDbContextFactory.Create(DatabaseName);
        var repository = new UserRepository(context);
        var user = CreateUserWithAuthor("manuel@portfolio.local", "Manuel");

        var result = await repository.Create(user);

        await using var verificationContext = PostgreSqlDbContextFactory.Create(DatabaseName);
        var persistedUser = await verificationContext.Users
            .Include(x => x.Author)
            .SingleAsync(x => x.Id == user.Id);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(user));
            Assert.That(persistedUser.Email, Is.EqualTo("manuel@portfolio.local"));
            Assert.That(persistedUser.Author.Name, Is.EqualTo("Manuel"));
        });
    }

    [Test]
    public async Task GetByEmail_ShouldReturnUserWithAuthorFromPostgreSql()
    {
        await using var context = PostgreSqlDbContextFactory.Create(DatabaseName);
        var user = CreateUserWithAuthor("manuel@portfolio.local", "Manuel");

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repository = new UserRepository(context);

        var result = await repository.GetByEmail(user.Email);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Email, Is.EqualTo("manuel@portfolio.local"));
            Assert.That(result.Author.Name, Is.EqualTo("Manuel"));
        });
    }

    [Test]
    public async Task Create_ShouldThrowInfrastructurePersistenceException_WhenEmailAlreadyExistsInPostgreSql()
    {
        await using var context = PostgreSqlDbContextFactory.Create(DatabaseName);
        var repository = new UserRepository(context);
        var firstUser = CreateUserWithAuthor("manuel@portfolio.local", "Manuel");
        var duplicateUser = CreateUserWithAuthor("manuel@portfolio.local", "Maria");

        await repository.Create(firstUser);

        Assert.That(
            async () => await repository.Create(duplicateUser),
            Throws.InstanceOf<InfrastructurePersistenceException>());
    }

    private static User CreateUserWithAuthor(string email, string authorName)
    {
        var userId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var user = new User(
            email,
            "hash",
            new DateTimeOffset(2026, 06, 24, 0, 0, 0, TimeSpan.Zero),
            UserRole.User,
            true)
        {
            Id = userId
        };

        var author = new Author(authorName)
        {
            Id = authorId,
            UserId = userId,
            User = user
        };

        user.Author = author;

        return user;
    }
}
