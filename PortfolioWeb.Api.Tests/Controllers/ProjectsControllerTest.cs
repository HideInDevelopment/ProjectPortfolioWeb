using Microsoft.AspNetCore.Mvc;
using Moq;
using PortfolioWeb.Api.Controllers;
using PortfolioWeb.Application.Contract.DTOs;
using PortfolioWeb.Application.Contract.Services;

namespace PortfolioWeb.Api.Tests.Controllers;

public class ProjectsControllerTest
{
    [Test]
    public async Task GetAll_ShouldReturnOkWithProjects()
    {
        var projectService = new Mock<IProjectService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var projects = new List<ProjectDTO>
        {
            new() { Id = Guid.NewGuid(), Title = "PortfolioWeb" }
        };

        projectService
            .Setup(service => service.GetAll(cancellationToken))
            .ReturnsAsync(projects);

        var controller = new ProjectsController(projectService.Object);

        var result = await controller.GetAll(cancellationToken);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(projects));
        projectService.Verify(service => service.GetAll(cancellationToken), Times.Once);
    }

    [Test]
    public async Task GetById_ShouldReturnOkWithProject()
    {
        var projectService = new Mock<IProjectService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var projectId = Guid.NewGuid();
        var project = new ProjectDTO { Id = projectId, Title = "PortfolioWeb" };

        projectService
            .Setup(service => service.GetById(projectId, cancellationToken))
            .ReturnsAsync(project);

        var controller = new ProjectsController(projectService.Object);

        var result = await controller.GetById(projectId, cancellationToken);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(project));
        projectService.Verify(service => service.GetById(projectId, cancellationToken), Times.Once);
    }

    [Test]
    public async Task Create_ShouldReturnCreatedAtActionWithCreatedProject()
    {
        var projectService = new Mock<IProjectService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var createProjectDto = new CreateProjectDTO
        {
            Title = "PortfolioWeb",
            Description = "Personal portfolio website.",
            ReleaseDate = new DateTimeOffset(2026, 06, 17, 0, 0, 0, TimeSpan.Zero),
            Version = 1,
            AuthorId = Guid.NewGuid(),
            IsInDevelopment = true
        };
        var createdProject = new ProjectDTO { Id = Guid.NewGuid(), Title = "PortfolioWeb" };

        projectService
            .Setup(service => service.Create(createProjectDto, cancellationToken))
            .ReturnsAsync(createdProject);

        var controller = new ProjectsController(projectService.Object);

        var result = await controller.Create(createProjectDto, cancellationToken);
        var createdAtActionResult = result.Result as CreatedAtActionResult;

        Assert.Multiple(() =>
        {
            Assert.That(createdAtActionResult, Is.Not.Null);
            Assert.That(createdAtActionResult!.ActionName, Is.EqualTo(nameof(ProjectsController.GetById)));
            Assert.That(createdAtActionResult.RouteValues!["id"], Is.EqualTo(createdProject.Id));
            Assert.That(createdAtActionResult.Value, Is.SameAs(createdProject));
        });

        projectService.Verify(service => service.Create(createProjectDto, cancellationToken), Times.Once);
    }

    [Test]
    public async Task Update_ShouldReturnOkWithUpdatedProject()
    {
        var projectService = new Mock<IProjectService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var projectId = Guid.NewGuid();
        var updateProjectDto = new UpdateProjectDTO
        {
            Title = "Updated",
            Description = "Updated description",
            Version = 2,
            IsInDevelopment = false
        };
        var updatedProject = new ProjectDTO { Id = projectId, Title = "Updated" };

        projectService
            .Setup(service => service.Update(projectId, updateProjectDto, cancellationToken))
            .ReturnsAsync(updatedProject);

        var controller = new ProjectsController(projectService.Object);

        var result = await controller.Update(projectId, updateProjectDto, cancellationToken);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(updatedProject));
        projectService.Verify(service => service.Update(projectId, updateProjectDto, cancellationToken), Times.Once);
    }

    [Test]
    public async Task Delete_ShouldReturnNoContent()
    {
        var projectService = new Mock<IProjectService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var projectId = Guid.NewGuid();

        projectService
            .Setup(service => service.Delete(projectId, cancellationToken))
            .Returns(Task.CompletedTask);

        var controller = new ProjectsController(projectService.Object);

        var result = await controller.Delete(projectId, cancellationToken);

        Assert.That(result, Is.TypeOf<NoContentResult>());
        projectService.Verify(service => service.Delete(projectId, cancellationToken), Times.Once);
    }
}
