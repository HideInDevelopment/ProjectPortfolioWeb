using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
        var authService = new Mock<IAuthService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var projects = new List<ProjectDTO>
        {
            new() { Id = Guid.NewGuid(), Title = "PortfolioWeb" }
        };

        projectService
            .Setup(service => service.GetAll(cancellationToken))
            .ReturnsAsync(projects);

        var controller = new ProjectsController(projectService.Object, authService.Object);

        var result = await controller.GetAll(cancellationToken);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(projects));
        projectService.Verify(service => service.GetAll(cancellationToken), Times.Once);
    }

    [Test]
    public async Task GetById_ShouldReturnOkWithProject()
    {
        var projectService = new Mock<IProjectService>();
        var authService = new Mock<IAuthService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var projectId = Guid.NewGuid();
        var project = new ProjectDTO { Id = projectId, Title = "PortfolioWeb" };

        projectService
            .Setup(service => service.GetById(projectId, cancellationToken))
            .ReturnsAsync(project);

        var controller = new ProjectsController(projectService.Object, authService.Object);

        var result = await controller.GetById(projectId, cancellationToken);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(project));
        projectService.Verify(service => service.GetById(projectId, cancellationToken), Times.Once);
    }

    [Test]
    public async Task Create_ShouldReturnCreatedAtActionWithCreatedProject()
    {
        var projectService = new Mock<IProjectService>();
        var authService = new Mock<IAuthService>();
        var cancellationToken = new CancellationTokenSource().Token;
        const string email = "manuel@portfolio.local";
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

        authService
            .Setup(service => service.EnsureCurrentUserIsActive(email, cancellationToken))
            .Returns(Task.CompletedTask);

        projectService
            .Setup(service => service.Create(createProjectDto, createProjectDto.AuthorId, cancellationToken))
            .ReturnsAsync(createdProject);

        var controller = new ProjectsController(projectService.Object, authService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreateUser(createProjectDto.AuthorId, email)
                }
            }
        };

        var result = await controller.Create(createProjectDto, cancellationToken);
        var createdAtActionResult = result.Result as CreatedAtActionResult;

        Assert.Multiple(() =>
        {
            Assert.That(createdAtActionResult, Is.Not.Null);
            Assert.That(createdAtActionResult!.ActionName, Is.EqualTo(nameof(ProjectsController.GetById)));
            Assert.That(createdAtActionResult.RouteValues!["id"], Is.EqualTo(createdProject.Id));
            Assert.That(createdAtActionResult.Value, Is.SameAs(createdProject));
        });

        authService.Verify(service => service.EnsureCurrentUserIsActive(email, cancellationToken), Times.Once);
        projectService.Verify(service => service.Create(createProjectDto, createProjectDto.AuthorId, cancellationToken), Times.Once);
    }

    [Test]
    public async Task Create_ShouldReturnUnauthorized_WhenAuthorIdClaimIsMissing()
    {
        var projectService = new Mock<IProjectService>();
        var authService = new Mock<IAuthService>();
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
        authService
            .Setup(service => service.EnsureCurrentUserIsActive("manuel@portfolio.local", cancellationToken))
            .Returns(Task.CompletedTask);
        var controller = new ProjectsController(projectService.Object, authService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreateUser(null, "manuel@portfolio.local")
                }
            }
        };

        var result = await controller.Create(createProjectDto, cancellationToken);

        Assert.That(result.Result, Is.TypeOf<UnauthorizedResult>());
        authService.Verify(service => service.EnsureCurrentUserIsActive("manuel@portfolio.local", cancellationToken), Times.Once);
        projectService.Verify(service => service.Create(It.IsAny<CreateProjectDTO>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Create_ShouldReturnUnauthorized_WhenEmailClaimIsMissing()
    {
        var projectService = new Mock<IProjectService>();
        var authService = new Mock<IAuthService>();
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
        authService
            .Setup(service => service.EnsureCurrentUserIsActive("manuel@portfolio.local", cancellationToken))
            .Returns(Task.CompletedTask);
        var controller = new ProjectsController(projectService.Object, authService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            }
        };

        var result = await controller.Create(createProjectDto, cancellationToken);

        Assert.That(result.Result, Is.TypeOf<UnauthorizedResult>());
        authService.Verify(service => service.EnsureCurrentUserIsActive(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        projectService.Verify(service => service.Create(It.IsAny<CreateProjectDTO>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Update_ShouldReturnOkWithUpdatedProject()
    {
        var projectService = new Mock<IProjectService>();
        var authService = new Mock<IAuthService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var projectId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        const string email = "manuel@portfolio.local";
        var updateProjectDto = new UpdateProjectDTO
        {
            Title = "Updated",
            Description = "Updated description",
            Version = 2,
            IsInDevelopment = false
        };
        var updatedProject = new ProjectDTO { Id = projectId, Title = "Updated" };

        authService
            .Setup(service => service.EnsureCurrentUserIsActive(email, cancellationToken))
            .Returns(Task.CompletedTask);

        projectService
            .Setup(service => service.Update(projectId, updateProjectDto, authorId, cancellationToken))
            .ReturnsAsync(updatedProject);

        var controller = new ProjectsController(projectService.Object, authService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreateUser(authorId, email)
                }
            }
        };

        var result = await controller.Update(projectId, updateProjectDto, cancellationToken);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(updatedProject));
        authService.Verify(service => service.EnsureCurrentUserIsActive(email, cancellationToken), Times.Once);
        projectService.Verify(service => service.Update(projectId, updateProjectDto, authorId, cancellationToken), Times.Once);
    }

    [Test]
    public async Task Update_ShouldReturnUnauthorized_WhenAuthorIdClaimIsMissing()
    {
        var projectService = new Mock<IProjectService>();
        var authService = new Mock<IAuthService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var projectId = Guid.NewGuid();
        var updateProjectDto = new UpdateProjectDTO
        {
            Title = "Updated",
            Description = "Updated description",
            Version = 2,
            IsInDevelopment = false
        };
        var controller = new ProjectsController(projectService.Object, authService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreateUser(null, "manuel@portfolio.local")
                }
            }
        };

        var result = await controller.Update(projectId, updateProjectDto, cancellationToken);

        Assert.That(result.Result, Is.TypeOf<UnauthorizedResult>());
        authService.Verify(service => service.EnsureCurrentUserIsActive("manuel@portfolio.local", cancellationToken), Times.Once);
        projectService.Verify(service => service.Update(It.IsAny<Guid>(), It.IsAny<UpdateProjectDTO>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Delete_ShouldReturnNoContent()
    {
        var projectService = new Mock<IProjectService>();
        var authService = new Mock<IAuthService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var projectId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        const string email = "manuel@portfolio.local";

        authService
            .Setup(service => service.EnsureCurrentUserIsActive(email, cancellationToken))
            .Returns(Task.CompletedTask);

        projectService
            .Setup(service => service.Delete(projectId, authorId, cancellationToken))
            .Returns(Task.CompletedTask);

        var controller = new ProjectsController(projectService.Object, authService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreateUser(authorId, email)
                }
            }
        };

        var result = await controller.Delete(projectId, cancellationToken);

        Assert.That(result, Is.TypeOf<NoContentResult>());
        authService.Verify(service => service.EnsureCurrentUserIsActive(email, cancellationToken), Times.Once);
        projectService.Verify(service => service.Delete(projectId, authorId, cancellationToken), Times.Once);
    }

    [Test]
    public async Task Delete_ShouldReturnUnauthorized_WhenAuthorIdClaimIsMissing()
    {
        var projectService = new Mock<IProjectService>();
        var authService = new Mock<IAuthService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var projectId = Guid.NewGuid();
        authService
            .Setup(service => service.EnsureCurrentUserIsActive("manuel@portfolio.local", cancellationToken))
            .Returns(Task.CompletedTask);
        var controller = new ProjectsController(projectService.Object, authService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreateUser(null, "manuel@portfolio.local")
                }
            }
        };

        var result = await controller.Delete(projectId, cancellationToken);

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
        authService.Verify(service => service.EnsureCurrentUserIsActive("manuel@portfolio.local", cancellationToken), Times.Once);
        projectService.Verify(service => service.Delete(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static ClaimsPrincipal CreateUser(Guid? authorId, string? email)
    {
        var claims = new List<Claim>();

        if (!string.IsNullOrWhiteSpace(email))
        {
            claims.Add(new Claim(ClaimTypes.Email, email));
        }

        if (authorId.HasValue)
        {
            claims.Add(new Claim("authorId", authorId.Value.ToString()));
        }

        return new ClaimsPrincipal(
            new ClaimsIdentity(
            claims,
            authenticationType: "Test"));
    }
}
