using Microsoft.Extensions.Logging;
using Moq;
using PortfolioWeb.Application.Contract.DTOs;
using PortfolioWeb.Application.Contract.Exceptions.Author;
using PortfolioWeb.Application.Contract.Exceptions.Project;
using PortfolioWeb.Application.Services;
using PortfolioWeb.Core.Contracts.Repositories;
using PortfolioWeb.Domain.Entities;

namespace PortfolioWeb.Application.Tests.Services;

public class ProjectServiceTest
{
    private Mock<IProjectRepository> _projectRepositoryMock = null!;
    private Mock<IAuthorRepository> _authorRepositoryMock = null!;
    private Mock<ILogger<ProjectService>> _loggerMock = null!;
    private ProjectService _projectService = null!;

    [SetUp]
    public void SetUp()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _authorRepositoryMock = new Mock<IAuthorRepository>();
        _loggerMock = new Mock<ILogger<ProjectService>>();

        _projectService = new ProjectService(
            _projectRepositoryMock.Object,
            _authorRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task GetById_ShouldReturnProjectDto_WhenProjectExists()
    {
        var projectId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var releaseDate = new DateTimeOffset(2026, 06, 17, 0, 0, 0, TimeSpan.Zero);
        var project = CreateProject(
            projectId,
            "VetApp",
            "Vet manager for schedule appointments.",
            releaseDate,
            1,
            authorId,
            true);

        _projectRepositoryMock
            .Setup(x => x.GetById(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var result = await _projectService.GetById(projectId);

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(projectId));
            Assert.That(result.Title, Is.EqualTo("VetApp"));
            Assert.That(result.Description, Is.EqualTo("Vet manager for schedule appointments."));
            Assert.That(result.ReleaseDate, Is.EqualTo(releaseDate));
            Assert.That(result.Version, Is.EqualTo(1));
            Assert.That(result.AuthorId, Is.EqualTo(authorId));
            Assert.That(result.IsInDevelopment, Is.True);
        });
    }

    [Test]
    public void GetById_ShouldThrowInvalidProjectIdException_WhenIdIsEmpty()
    {
        var exception = Assert.ThrowsAsync<InvalidProjectIdException>(
            async () => await _projectService.GetById(Guid.Empty));

        Assert.That(exception!.Message, Is.EqualTo("The provided project id is not valid."));
        _projectRepositoryMock.Verify(
            x => x.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public void GetById_ShouldThrowProjectNotFoundException_WhenProjectDoesNotExist()
    {
        var projectId = Guid.NewGuid();

        _projectRepositoryMock
            .Setup(x => x.GetById(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        var exception = Assert.ThrowsAsync<ProjectNotFoundException>(
            async () => await _projectService.GetById(projectId));

        Assert.That(exception!.Message, Is.EqualTo($"The project with id '{projectId}' was not found."));
    }

    [Test]
    public async Task GetAll_ShouldReturnMappedProjects_WhenRepositoryReturnsProjects()
    {
        var projects = new List<Project>
        {
            CreateProject(
                Guid.NewGuid(),
                "VetApp",
                "Vet manager for schedule appointments.",
                new DateTimeOffset(2026, 06, 17, 0, 0, 0, TimeSpan.Zero),
                1,
                Guid.NewGuid(),
                true),
            CreateProject(
                Guid.NewGuid(),
                "PortfolioWeb",
                "Personal portfolio website.",
                new DateTimeOffset(2026, 07, 01, 0, 0, 0, TimeSpan.Zero),
                2,
                Guid.NewGuid(),
                false)
        };

        _projectRepositoryMock
            .Setup(x => x.GetAll(It.IsAny<CancellationToken>()))
            .ReturnsAsync(projects);

        var result = await _projectService.GetAll();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(result[0].Id, Is.EqualTo(projects[0].Id));
            Assert.That(result[0].Title, Is.EqualTo(projects[0].Title));
            Assert.That(result[1].Id, Is.EqualTo(projects[1].Id));
            Assert.That(result[1].Title, Is.EqualTo(projects[1].Title));
        });
    }

    [Test]
    public void Create_ShouldThrowInvalidAuthorIdException_WhenAuthorIdIsEmpty()
    {
        var projectDto = new CreateProjectDTO
        {
            Title = "VetApp",
            Description = "Vet manager for schedule appointments.",
            ReleaseDate = new DateTimeOffset(2026, 06, 17, 0, 0, 0, TimeSpan.Zero),
            Version = 1,
            AuthorId = Guid.Empty,
            IsInDevelopment = true
        };

        var exception = Assert.ThrowsAsync<InvalidAuthorIdException>(
            async () => await _projectService.Create(projectDto));

        Assert.That(exception!.Message, Is.EqualTo("The provided author id is not valid."));
        _authorRepositoryMock.Verify(
            x => x.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _projectRepositoryMock.Verify(
            x => x.Create(It.IsAny<Project>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public void Create_ShouldThrowReferencedAuthorNotFoundException_WhenAuthorDoesNotExist()
    {
        var authorId = Guid.NewGuid();
        var projectDto = new CreateProjectDTO
        {
            Title = "VetApp",
            Description = "Vet manager for schedule appointments.",
            ReleaseDate = new DateTimeOffset(2026, 06, 17, 0, 0, 0, TimeSpan.Zero),
            Version = 1,
            AuthorId = authorId,
            IsInDevelopment = true
        };

        _authorRepositoryMock
            .Setup(x => x.GetById(authorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Author?)null);

        var exception = Assert.ThrowsAsync<ReferencedAuthorNotFoundException>(
            async () => await _projectService.Create(projectDto));

        Assert.That(exception!.Message, Is.EqualTo($"The referenced author with id '{authorId}' was not found."));
        _projectRepositoryMock.Verify(
            x => x.Create(It.IsAny<Project>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Create_ShouldMapCreateProjectDtoAndReturnCreatedProjectDto_WhenAuthorExists()
    {
        var authorId = Guid.NewGuid();
        var releaseDate = new DateTimeOffset(2026, 06, 17, 0, 0, 0, TimeSpan.Zero);
        var projectDto = new CreateProjectDTO
        {
            Title = "VetApp",
            Description = "Vet manager for schedule appointments.",
            ReleaseDate = releaseDate,
            Version = 1,
            AuthorId = authorId,
            IsInDevelopment = true
        };

        var author = new Author("Manuel")
        {
            Id = authorId
        };

        Project? createdProjectArgument = null;
        var persistedProject = CreateProject(
            Guid.NewGuid(),
            "VetApp",
            "Vet manager for schedule appointments.",
            releaseDate,
            1,
            authorId,
            true);

        _authorRepositoryMock
            .Setup(x => x.GetById(authorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(author);

        _projectRepositoryMock
            .Setup(x => x.Create(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .Callback<Project, CancellationToken>((project, _) => createdProjectArgument = project)
            .ReturnsAsync(persistedProject);

        var result = await _projectService.Create(projectDto);

        Assert.That(createdProjectArgument, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(createdProjectArgument!.Id, Is.EqualTo(Guid.Empty));
            Assert.That(createdProjectArgument.Title, Is.EqualTo(projectDto.Title));
            Assert.That(createdProjectArgument.Description, Is.EqualTo(projectDto.Description));
            Assert.That(createdProjectArgument.ReleaseDate, Is.EqualTo(projectDto.ReleaseDate));
            Assert.That(createdProjectArgument.Version, Is.EqualTo(projectDto.Version));
            Assert.That(createdProjectArgument.AuthorId, Is.EqualTo(projectDto.AuthorId));
            Assert.That(createdProjectArgument.IsInDevelopment, Is.EqualTo(projectDto.IsInDevelopment));
            Assert.That(result.Id, Is.EqualTo(persistedProject.Id));
            Assert.That(result.AuthorId, Is.EqualTo(authorId));
        });
    }

    [Test]
    public void Update_ShouldThrowInvalidProjectIdException_WhenIdIsEmpty()
    {
        var projectDto = new UpdateProjectDTO
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Version = 2,
            IsInDevelopment = false
        };

        var exception = Assert.ThrowsAsync<InvalidProjectIdException>(
            async () => await _projectService.Update(Guid.Empty, projectDto));

        Assert.That(exception!.Message, Is.EqualTo("The provided project id is not valid."));
        _projectRepositoryMock.Verify(
            x => x.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _projectRepositoryMock.Verify(
            x => x.Update(It.IsAny<Project>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public void Update_ShouldThrowProjectNotFoundException_WhenProjectDoesNotExistDuringRetrieval()
    {
        var projectId = Guid.NewGuid();
        var projectDto = new UpdateProjectDTO
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Version = 2,
            IsInDevelopment = false
        };

        _projectRepositoryMock
            .Setup(x => x.GetById(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        var exception = Assert.ThrowsAsync<ProjectNotFoundException>(
            async () => await _projectService.Update(projectId, projectDto));

        Assert.That(exception!.Message, Is.EqualTo($"The project with id '{projectId}' was not found."));
        _projectRepositoryMock.Verify(
            x => x.Update(It.IsAny<Project>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Update_ShouldOnlyModifyMutableFieldsAndReturnUpdatedProjectDto_WhenProjectExists()
    {
        var projectId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var originalReleaseDate = new DateTimeOffset(2026, 06, 17, 0, 0, 0, TimeSpan.Zero);
        var existingProject = CreateProject(
            projectId,
            "Original Title",
            "Original Description",
            originalReleaseDate,
            1,
            authorId,
            false);

        var projectDto = new UpdateProjectDTO
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Version = 2,
            IsInDevelopment = true
        };

        Project? updatedProjectArgument = null;
        var persistedProject = CreateProject(
            projectId,
            "Updated Title",
            "Updated Description",
            originalReleaseDate,
            2,
            authorId,
            true);

        _projectRepositoryMock
            .Setup(x => x.GetById(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProject);

        _projectRepositoryMock
            .Setup(x => x.Update(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .Callback<Project, CancellationToken>((project, _) => updatedProjectArgument = project)
            .ReturnsAsync(persistedProject);

        var result = await _projectService.Update(projectId, projectDto);

        Assert.That(updatedProjectArgument, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(updatedProjectArgument!.Id, Is.EqualTo(projectId));
            Assert.That(updatedProjectArgument.Title, Is.EqualTo(projectDto.Title));
            Assert.That(updatedProjectArgument.Description, Is.EqualTo(projectDto.Description));
            Assert.That(updatedProjectArgument.Version, Is.EqualTo(projectDto.Version));
            Assert.That(updatedProjectArgument.IsInDevelopment, Is.EqualTo(projectDto.IsInDevelopment));
            Assert.That(updatedProjectArgument.AuthorId, Is.EqualTo(authorId));
            Assert.That(updatedProjectArgument.ReleaseDate, Is.EqualTo(originalReleaseDate));
            Assert.That(result.Id, Is.EqualTo(projectId));
            Assert.That(result.Title, Is.EqualTo(projectDto.Title));
            Assert.That(result.AuthorId, Is.EqualTo(authorId));
            Assert.That(result.ReleaseDate, Is.EqualTo(originalReleaseDate));
        });
    }

    [Test]
    public void Update_ShouldThrowProjectNotFoundException_WhenProjectDoesNotExistDuringPersistence()
    {
        var projectId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var existingProject = CreateProject(
            projectId,
            "Original Title",
            "Original Description",
            new DateTimeOffset(2026, 06, 17, 0, 0, 0, TimeSpan.Zero),
            1,
            authorId,
            false);

        var projectDto = new UpdateProjectDTO
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Version = 2,
            IsInDevelopment = true
        };

        _projectRepositoryMock
            .Setup(x => x.GetById(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProject);

        _projectRepositoryMock
            .Setup(x => x.Update(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        var exception = Assert.ThrowsAsync<ProjectNotFoundException>(
            async () => await _projectService.Update(projectId, projectDto));

        Assert.That(exception!.Message, Is.EqualTo($"The project with id '{projectId}' was not found."));
    }

    [Test]
    public void Delete_ShouldThrowInvalidProjectIdException_WhenIdIsEmpty()
    {
        var exception = Assert.ThrowsAsync<InvalidProjectIdException>(
            async () => await _projectService.Delete(Guid.Empty));

        Assert.That(exception!.Message, Is.EqualTo("The provided project id is not valid."));
        _projectRepositoryMock.Verify(
            x => x.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _projectRepositoryMock.Verify(
            x => x.Delete(It.IsAny<Project>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public void Delete_ShouldThrowProjectNotFoundException_WhenProjectDoesNotExist()
    {
        var projectId = Guid.NewGuid();

        _projectRepositoryMock
            .Setup(x => x.GetById(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        var exception = Assert.ThrowsAsync<ProjectNotFoundException>(
            async () => await _projectService.Delete(projectId));

        Assert.That(exception!.Message, Is.EqualTo($"The project with id '{projectId}' was not found."));
        _projectRepositoryMock.Verify(
            x => x.Delete(It.IsAny<Project>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Delete_ShouldReturnTrue_WhenProjectIsDeletedSuccessfully()
    {
        var projectId = Guid.NewGuid();
        var project = CreateProject(
            projectId,
            "VetApp",
            "Vet manager for schedule appointments.",
            new DateTimeOffset(2026, 06, 17, 0, 0, 0, TimeSpan.Zero),
            1,
            Guid.NewGuid(),
            true);

        _projectRepositoryMock
            .Setup(x => x.GetById(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _projectRepositoryMock
            .Setup(x => x.Delete(project, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _projectService.Delete(projectId);

        Assert.That(result, Is.True);
        _projectRepositoryMock.Verify(
            x => x.Delete(project, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static Project CreateProject(
        Guid id,
        string title,
        string description,
        DateTimeOffset releaseDate,
        int version,
        Guid authorId,
        bool isInDevelopment)
    {
        return new Project(
            title,
            description,
            releaseDate,
            version,
            authorId,
            isInDevelopment)
        {
            Id = id
        };
    }
}
