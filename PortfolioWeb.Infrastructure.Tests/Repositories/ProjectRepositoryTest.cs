using Microsoft.EntityFrameworkCore;
using PortfolioWeb.Domain.Entities;
using PortfolioWeb.Infrastructure.Repositories;
using PortfolioWeb.Infrastructure.Tests.Helpers;

namespace PortfolioWeb.Infrastructure.Tests.Repositories;

public class ProjectRepositoryTest
{
    [Test]
    public void GetById_ShouldThrowOperationCanceledException_WhenCancellationIsRequested()
    {
        using var context = InMemoryDbContextFactory.Create();
        var repository = new ProjectRepository(context);
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        Assert.That(
            async () => await repository.GetById(Guid.NewGuid(), cancellationTokenSource.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public async Task GetById_ShouldReturnProject_WhenProjectExists()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repository = new ProjectRepository(context);
        var authorId = Guid.NewGuid();
        var project = CreateProject(Guid.NewGuid(), authorId, "PortfolioWeb", 1, true);

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var result = await repository.GetById(project.Id);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(project.Id));
            Assert.That(result.Title, Is.EqualTo("PortfolioWeb"));
            Assert.That(result.AuthorId, Is.EqualTo(authorId));
        });
    }

    [Test]
    public async Task GetById_ShouldReturnNull_WhenProjectDoesNotExist()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repository = new ProjectRepository(context);

        var result = await repository.GetById(Guid.NewGuid());

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetAll_ShouldThrowOperationCanceledException_WhenCancellationIsRequested()
    {
        using var context = InMemoryDbContextFactory.Create();
        var repository = new ProjectRepository(context);
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        Assert.That(
            async () => await repository.GetAll(cancellationTokenSource.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public async Task GetAll_ShouldReturnAllProjects()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repository = new ProjectRepository(context);
        var authorId = Guid.NewGuid();

        context.Projects.Add(CreateProject(Guid.NewGuid(), authorId, "Project 1", 1, true));
        context.Projects.Add(CreateProject(Guid.NewGuid(), authorId, "Project 2", 2, false));
        await context.SaveChangesAsync();

        var result = await repository.GetAll();

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Select(project => project.Title), Is.EquivalentTo(["Project 1", "Project 2"]));
        });
    }

    [Test]
    public async Task Create_ShouldPersistProjectAndReturnIt()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repository = new ProjectRepository(context);
        var authorId = Guid.NewGuid();
        var project = CreateProject(Guid.NewGuid(), authorId, "PortfolioWeb", 1, true);

        var result = await repository.Create(project);
        var persistedProject = await context.Projects.FindAsync(project.Id);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(project));
            Assert.That(persistedProject, Is.Not.Null);
            Assert.That(persistedProject!.Title, Is.EqualTo("PortfolioWeb"));
        });
    }

    [Test]
    public void Create_ShouldThrowOperationCanceledException_WhenCancellationIsRequested()
    {
        using var context = InMemoryDbContextFactory.Create();
        var repository = new ProjectRepository(context);
        var project = CreateProject(Guid.NewGuid(), Guid.NewGuid(), "PortfolioWeb", 1, true);
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        Assert.That(
            async () => await repository.Create(project, cancellationTokenSource.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public async Task Update_ShouldPersistUpdatedValues_WhenProjectExists()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repository = new ProjectRepository(context);
        var projectId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var existingProject = CreateProject(projectId, authorId, "Original", 1, true);

        context.Projects.Add(existingProject);
        await context.SaveChangesAsync();

        var updatedProject = CreateProject(projectId, authorId, "Updated", 2, false);

        var result = await repository.Update(updatedProject);
        var persistedProject = await context.Projects.SingleAsync(project => project.Id == projectId);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Title, Is.EqualTo("Updated"));
            Assert.That(result.Version, Is.EqualTo(2));
            Assert.That(result.IsInDevelopment, Is.False);
            Assert.That(persistedProject.Title, Is.EqualTo("Updated"));
            Assert.That(persistedProject.Version, Is.EqualTo(2));
            Assert.That(persistedProject.IsInDevelopment, Is.False);
            Assert.That(persistedProject.AuthorId, Is.EqualTo(authorId));
        });
    }

    [Test]
    public void Update_ShouldThrowOperationCanceledException_WhenCancellationIsRequested()
    {
        using var context = InMemoryDbContextFactory.Create();
        var repository = new ProjectRepository(context);
        var project = CreateProject(Guid.NewGuid(), Guid.NewGuid(), "Updated", 2, false);
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        Assert.That(
            async () => await repository.Update(project, cancellationTokenSource.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public async Task Update_ShouldReturnNull_WhenProjectDoesNotExist()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repository = new ProjectRepository(context);
        var project = CreateProject(Guid.NewGuid(), Guid.NewGuid(), "Updated", 2, false);

        var result = await repository.Update(project);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Delete_ShouldRemoveProject_WhenProjectExists()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repository = new ProjectRepository(context);
        var project = CreateProject(Guid.NewGuid(), Guid.NewGuid(), "PortfolioWeb", 1, true);

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        await repository.Delete(project);
        var persistedProject = await context.Projects.FindAsync(project.Id);

        Assert.Multiple(() =>
        {
            Assert.That(persistedProject, Is.Null);
        });
    }

    [Test]
    public async Task Delete_ShouldThrowOperationCanceledException_WhenCancellationIsRequested()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repository = new ProjectRepository(context);
        var project = CreateProject(Guid.NewGuid(), Guid.NewGuid(), "PortfolioWeb", 1, true);

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        Assert.That(
            async () => await repository.Delete(project, cancellationTokenSource.Token),
            Throws.InstanceOf<OperationCanceledException>());
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
}
