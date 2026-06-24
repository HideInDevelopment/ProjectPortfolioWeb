using Microsoft.EntityFrameworkCore;
using PortfolioWeb.Domain.Entities;
using PortfolioWeb.Infrastructure.Repositories;
using PortfolioWeb.Infrastructure.Tests.Helpers;

namespace PortfolioWeb.Infrastructure.Tests.Integration.Repositories;

[NonParallelizable]
public class AuthorRepositoryPostgreSqlIntegrationTest
{
    private const string DatabaseName = "portfolio_web_author_integration_tests";

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
    public async Task Create_ShouldPersistAuthorInPostgreSql()
    {
        await using var context = PostgreSqlDbContextFactory.Create(DatabaseName);
        var repository = new AuthorRepository(context);
        var author = CreateAuthor(Guid.NewGuid(), "Manuel");

        var result = await repository.Create(author);

        await using var verificationContext = PostgreSqlDbContextFactory.Create(DatabaseName);
        var persistedAuthor = await verificationContext.Authors.SingleAsync(x => x.Id == author.Id);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(author));
            Assert.That(persistedAuthor.Name, Is.EqualTo("Manuel"));
        });
    }

    [Test]
    public async Task GetById_ShouldReturnAuthorWithProjectsFromPostgreSql()
    {
        await using var context = PostgreSqlDbContextFactory.Create(DatabaseName);
        var authorId = Guid.NewGuid();
        var author = CreateAuthor(authorId, "Manuel", CreateProject(authorId, "PortfolioWeb"));

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        var repository = new AuthorRepository(context);

        var result = await repository.GetById(authorId);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Manuel"));
            Assert.That(result.Projects, Has.Count.EqualTo(1));
            Assert.That(result.Projects[0].Title, Is.EqualTo("PortfolioWeb"));
        });
    }

    [Test]
    public async Task Update_ShouldPersistUpdatedAuthorInPostgreSql()
    {
        await using var context = PostgreSqlDbContextFactory.Create(DatabaseName);
        var authorId = Guid.NewGuid();
        var existingAuthor = CreateAuthor(authorId, "Original Name", CreateProject(authorId, "Existing Project"));

        context.Authors.Add(existingAuthor);
        await context.SaveChangesAsync();

        var repository = new AuthorRepository(context);
        var updatedAuthor = new Author("Updated Name")
        {
            Id = authorId
        };

        var result = await repository.Update(updatedAuthor);

        await using var verificationContext = PostgreSqlDbContextFactory.Create(DatabaseName);
        var persistedAuthor = await verificationContext.Authors
            .Include(author => author.Projects)
            .SingleAsync(author => author.Id == authorId);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Updated Name"));
            Assert.That(persistedAuthor.Name, Is.EqualTo("Updated Name"));
            Assert.That(persistedAuthor.Projects, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task Delete_ShouldCascadeProjectsInPostgreSql()
    {
        await using var context = PostgreSqlDbContextFactory.Create(DatabaseName);
        var authorId = Guid.NewGuid();
        var author = CreateAuthor(authorId, "Manuel", CreateProject(authorId, "PortfolioWeb"));

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        var repository = new AuthorRepository(context);

        await repository.Delete(author);

        await using var verificationContext = PostgreSqlDbContextFactory.Create(DatabaseName);
        var persistedAuthor = await verificationContext.Authors.FindAsync(authorId);
        var persistedProjects = await verificationContext.Projects.ToListAsync();

        Assert.Multiple(() =>
        {
            Assert.That(persistedAuthor, Is.Null);
            Assert.That(persistedProjects, Is.Empty);
        });
    }

    private static Author CreateAuthor(Guid authorId, string name, params Project[] projects)
    {
        var userId = Guid.NewGuid();
        var author = new Author(name)
        {
            Id = authorId,
            UserId = userId,
            User = CreateUser(userId)
        };

        foreach (var project in projects)
        {
            author.AddProject(project);
        }

        return author;
    }

    private static Project CreateProject(Guid authorId, string title)
    {
        return new Project(
            title,
            $"{title} description",
            new DateTimeOffset(2026, 06, 17, 0, 0, 0, TimeSpan.Zero),
            1,
            authorId,
            true)
        {
            Id = Guid.NewGuid()
        };
    }

    private static User CreateUser(Guid userId)
    {
        return new User(
            $"user-{userId:N}@portfolio.local",
            "hash",
            new DateTimeOffset(2026, 06, 17, 0, 0, 0, TimeSpan.Zero),
            UserRole.User,
            true)
        {
            Id = userId
        };
    }
}
