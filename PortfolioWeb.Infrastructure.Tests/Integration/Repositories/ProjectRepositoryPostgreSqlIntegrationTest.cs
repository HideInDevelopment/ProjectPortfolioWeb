using Microsoft.EntityFrameworkCore;
using PortfolioWeb.Domain.Entities;
using PortfolioWeb.Infrastructure.Repositories;
using PortfolioWeb.Infrastructure.Tests.Helpers;

namespace PortfolioWeb.Infrastructure.Tests.Integration.Repositories;

[NonParallelizable]
public class ProjectRepositoryPostgreSqlIntegrationTest
{
    private const string DatabaseName = "portfolio_web_project_integration_tests";

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
    public async Task Create_ShouldPersistProjectInPostgreSql()
    {
        await using var context = PostgreSqlDbContextFactory.Create(DatabaseName);
        var authorId = Guid.NewGuid();
        context.Authors.Add(CreateAuthor(authorId, "Manuel"));
        await context.SaveChangesAsync();

        var repository = new ProjectRepository(context);
        var project = CreateProject(Guid.NewGuid(), authorId, "PortfolioWeb", 1, true);

        var result = await repository.Create(project);

        await using var verificationContext = PostgreSqlDbContextFactory.Create(DatabaseName);
        var persistedProject = await verificationContext.Projects.SingleAsync(x => x.Id == project.Id);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(project));
            Assert.That(persistedProject.Title, Is.EqualTo("PortfolioWeb"));
            Assert.That(persistedProject.AuthorId, Is.EqualTo(authorId));
        });
    }

    [Test]
    public async Task GetAll_ShouldReturnProjectsFromPostgreSql()
    {
        await using var context = PostgreSqlDbContextFactory.Create(DatabaseName);
        var authorId = Guid.NewGuid();
        context.Authors.Add(CreateAuthor(authorId, "Manuel"));
        context.Projects.Add(CreateProject(Guid.NewGuid(), authorId, "Project 1", 1, true));
        context.Projects.Add(CreateProject(Guid.NewGuid(), authorId, "Project 2", 2, false));
        await context.SaveChangesAsync();

        var repository = new ProjectRepository(context);

        var result = await repository.GetAll();

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Select(project => project.Title), Is.EquivalentTo(["Project 1", "Project 2"]));
        });
    }

    [Test]
    public async Task Update_ShouldPersistProjectInPostgreSql()
    {
        await using var context = PostgreSqlDbContextFactory.Create(DatabaseName);
        var authorId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        context.Authors.Add(CreateAuthor(authorId, "Manuel"));
        context.Projects.Add(CreateProject(projectId, authorId, "Original", 1, true));
        await context.SaveChangesAsync();

        var repository = new ProjectRepository(context);
        var updatedProject = CreateProject(projectId, authorId, "Updated", 2, false);

        var result = await repository.Update(updatedProject);

        await using var verificationContext = PostgreSqlDbContextFactory.Create(DatabaseName);
        var persistedProject = await verificationContext.Projects.SingleAsync(project => project.Id == projectId);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Title, Is.EqualTo("Updated"));
            Assert.That(persistedProject.Title, Is.EqualTo("Updated"));
            Assert.That(persistedProject.Version, Is.EqualTo(2));
            Assert.That(persistedProject.IsInDevelopment, Is.False);
        });
    }

    [Test]
    public async Task Delete_ShouldRemoveProjectFromPostgreSql()
    {
        await using var context = PostgreSqlDbContextFactory.Create(DatabaseName);
        var authorId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        context.Authors.Add(CreateAuthor(authorId, "Manuel"));
        var project = CreateProject(projectId, authorId, "PortfolioWeb", 1, true);
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var repository = new ProjectRepository(context);

        await repository.Delete(project);

        await using var verificationContext = PostgreSqlDbContextFactory.Create(DatabaseName);
        var persistedProject = await verificationContext.Projects.FindAsync(projectId);

        Assert.That(persistedProject, Is.Null);
    }

    private static Project CreateProject(Guid projectId, Guid authorId, string title, int version, bool isInDevelopment)
    {
        return new Project(
            title,
            $"{title} description",
            new DateTimeOffset(2026, 06, 17, 0, 0, 0, TimeSpan.Zero),
            version,
            authorId,
            isInDevelopment)
        {
            Id = projectId
        };
    }

    private static Author CreateAuthor(Guid authorId, string name)
    {
        var userId = Guid.NewGuid();

        return new Author(name)
        {
            Id = authorId,
            UserId = userId,
            User = new User(
                $"user-{userId:N}@portfolio.local",
                "hash",
                new DateTimeOffset(2026, 06, 17, 0, 0, 0, TimeSpan.Zero),
                UserRole.User,
                true)
            {
                Id = userId
            }
        };
    }
}
