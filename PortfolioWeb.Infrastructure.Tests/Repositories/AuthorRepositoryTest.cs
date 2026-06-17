using Microsoft.EntityFrameworkCore;
using PortfolioWeb.Domain.Entities;
using PortfolioWeb.Infrastructure.Repositories;
using PortfolioWeb.Infrastructure.Tests.Helpers;

namespace PortfolioWeb.Infrastructure.Tests.Repositories;

public class AuthorRepositoryTest
{
    [Test]
    public async Task GetById_ShouldReturnAuthorWithProjects_WhenAuthorExists()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repository = new AuthorRepository(context);
        var authorId = Guid.NewGuid();
        var project = CreateProject(authorId, "PortfolioWeb");
        var author = CreateAuthor(authorId, "Manuel", project);

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        var result = await repository.GetById(authorId);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(authorId));
            Assert.That(result.Name, Is.EqualTo("Manuel"));
            Assert.That(result.Projects, Has.Count.EqualTo(1));
            Assert.That(result.Projects[0].Title, Is.EqualTo("PortfolioWeb"));
        });
    }

    [Test]
    public async Task GetById_ShouldReturnNull_WhenAuthorDoesNotExist()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repository = new AuthorRepository(context);

        var result = await repository.GetById(Guid.NewGuid());

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAll_ShouldReturnAllAuthorsWithProjects()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repository = new AuthorRepository(context);
        var firstAuthor = CreateAuthor(Guid.NewGuid(), "Author 1", CreateProject(Guid.NewGuid(), "Project 1"));
        var secondAuthorId = Guid.NewGuid();
        var secondAuthor = CreateAuthor(secondAuthorId, "Author 2", CreateProject(secondAuthorId, "Project 2"));

        context.Authors.Add(firstAuthor);
        context.Authors.Add(secondAuthor);
        await context.SaveChangesAsync();

        var result = await repository.GetAll();

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.All(author => author.Projects.Count == 1), Is.True);
        });
    }

    [Test]
    public async Task Create_ShouldPersistAuthorAndReturnIt()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repository = new AuthorRepository(context);
        var authorId = Guid.NewGuid();
        var author = CreateAuthor(authorId, "Manuel");

        var result = await repository.Create(author);
        var persistedAuthor = await context.Authors.FindAsync(authorId);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(author));
            Assert.That(persistedAuthor, Is.Not.Null);
            Assert.That(persistedAuthor!.Name, Is.EqualTo("Manuel"));
        });
    }

    [Test]
    public async Task Update_ShouldPersistUpdatedValues_WhenAuthorExists()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repository = new AuthorRepository(context);
        var authorId = Guid.NewGuid();
        var existingProject = CreateProject(authorId, "Existing Project");
        var existingAuthor = CreateAuthor(authorId, "Original Name", existingProject);

        context.Authors.Add(existingAuthor);
        await context.SaveChangesAsync();

        var updatedAuthor = new Author("Updated Name")
        {
            Id = authorId
        };

        var result = await repository.Update(updatedAuthor);
        var persistedAuthor = await context.Authors.Include(author => author.Projects).SingleAsync(author => author.Id == authorId);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Updated Name"));
            Assert.That(persistedAuthor.Name, Is.EqualTo("Updated Name"));
            Assert.That(persistedAuthor.Projects, Has.Count.EqualTo(1));
            Assert.That(persistedAuthor.Projects[0].Title, Is.EqualTo("Existing Project"));
        });
    }

    [Test]
    public async Task Update_ShouldReturnNull_WhenAuthorDoesNotExist()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repository = new AuthorRepository(context);
        var author = new Author("Updated Name")
        {
            Id = Guid.NewGuid()
        };

        var result = await repository.Update(author);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Delete_ShouldRemoveAuthorAndCascadeProjects_WhenAuthorExists()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repository = new AuthorRepository(context);
        var authorId = Guid.NewGuid();
        var author = CreateAuthor(authorId, "Manuel", CreateProject(authorId, "PortfolioWeb"));

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        var result = await repository.Delete(author);
        var persistedAuthor = await context.Authors.FindAsync(authorId);
        var projects = await context.Projects.ToListAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(persistedAuthor, Is.Null);
            Assert.That(projects, Is.Empty);
        });
    }

    private static Author CreateAuthor(Guid authorId, string name, params Project[] projects)
    {
        var author = new Author(name)
        {
            Id = authorId
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
}
