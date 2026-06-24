using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Moq;
using PortfolioWeb.Api.Controllers;
using PortfolioWeb.Application.Contract.DTOs;
using PortfolioWeb.Application.Contract.Services;

namespace PortfolioWeb.Api.Tests.Controllers;

public class AuthorsControllerTest
{
    [Test]
    public async Task GetAll_ShouldReturnOkWithAuthors()
    {
        var authorService = new Mock<IAuthorService>();
        var authService = new Mock<IAuthService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var authors = new List<AuthorDTO>
        {
            new() { Id = Guid.NewGuid(), Name = "Manuel" }
        };

        authorService
            .Setup(service => service.GetAll(cancellationToken))
            .ReturnsAsync(authors);

        var controller = new AuthorsController(authorService.Object, authService.Object);

        var result = await controller.GetAll(cancellationToken);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(authors));
        authorService.Verify(service => service.GetAll(cancellationToken), Times.Once);
    }

    [Test]
    public async Task GetById_ShouldReturnOkWithAuthor()
    {
        var authorService = new Mock<IAuthorService>();
        var authService = new Mock<IAuthService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var authorId = Guid.NewGuid();
        var author = new AuthorDTO { Id = authorId, Name = "Manuel" };

        authorService
            .Setup(service => service.GetById(authorId, cancellationToken))
            .ReturnsAsync(author);

        var controller = new AuthorsController(authorService.Object, authService.Object);

        var result = await controller.GetById(authorId, cancellationToken);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(author));
        authorService.Verify(service => service.GetById(authorId, cancellationToken), Times.Once);
    }

    [Test]
    public async Task Update_ShouldReturnOkWithUpdatedAuthor()
    {
        var authorService = new Mock<IAuthorService>();
        var authService = new Mock<IAuthService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var authorId = Guid.NewGuid();
        const string email = "manuel@portfolio.local";
        var persistedAuthorDto = new PersistAuthorDTO { Name = "Updated" };
        var updatedAuthor = new AuthorDTO { Id = authorId, Name = "Updated" };

        authService
            .Setup(service => service.EnsureCurrentUserIsActive(email, cancellationToken))
            .Returns(Task.CompletedTask);

        authorService
            .Setup(service => service.Update(authorId, persistedAuthorDto, authorId, cancellationToken))
            .ReturnsAsync(updatedAuthor);

        var controller = new AuthorsController(authorService.Object, authService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreateUser(authorId, email)
                }
            }
        };

        var result = await controller.Update(authorId, persistedAuthorDto, cancellationToken);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(updatedAuthor));
        authService.Verify(service => service.EnsureCurrentUserIsActive(email, cancellationToken), Times.Once);
        authorService.Verify(service => service.Update(authorId, persistedAuthorDto, authorId, cancellationToken), Times.Once);
    }

    [Test]
    public async Task Update_ShouldReturnUnauthorized_WhenAuthorIdClaimIsMissing()
    {
        var authorService = new Mock<IAuthorService>();
        var authService = new Mock<IAuthService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var authorId = Guid.NewGuid();
        var persistedAuthorDto = new PersistAuthorDTO { Name = "Updated" };
        authService
            .Setup(service => service.EnsureCurrentUserIsActive("manuel@portfolio.local", cancellationToken))
            .Returns(Task.CompletedTask);
        var controller = new AuthorsController(authorService.Object, authService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreateUser(null, "manuel@portfolio.local")
                }
            }
        };

        var result = await controller.Update(authorId, persistedAuthorDto, cancellationToken);

        Assert.That(result.Result, Is.TypeOf<UnauthorizedResult>());
        authService.Verify(service => service.EnsureCurrentUserIsActive("manuel@portfolio.local", cancellationToken), Times.Once);
        authorService.Verify(service => service.Update(It.IsAny<Guid>(), It.IsAny<PersistAuthorDTO>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Update_ShouldReturnUnauthorized_WhenEmailClaimIsMissing()
    {
        var authorService = new Mock<IAuthorService>();
        var authService = new Mock<IAuthService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var authorId = Guid.NewGuid();
        var persistedAuthorDto = new PersistAuthorDTO { Name = "Updated" };
        var controller = new AuthorsController(authorService.Object, authService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            }
        };

        var result = await controller.Update(authorId, persistedAuthorDto, cancellationToken);

        Assert.That(result.Result, Is.TypeOf<UnauthorizedResult>());
        authService.Verify(service => service.EnsureCurrentUserIsActive(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        authorService.Verify(service => service.Update(It.IsAny<Guid>(), It.IsAny<PersistAuthorDTO>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Delete_ShouldReturnNoContent()
    {
        var authorService = new Mock<IAuthorService>();
        var authService = new Mock<IAuthService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var authorId = Guid.NewGuid();
        const string email = "manuel@portfolio.local";

        authService
            .Setup(service => service.EnsureCurrentUserIsActive(email, cancellationToken))
            .Returns(Task.CompletedTask);

        authorService
            .Setup(service => service.Delete(authorId, authorId, cancellationToken))
            .Returns(Task.CompletedTask);

        var controller = new AuthorsController(authorService.Object, authService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreateUser(authorId, email)
                }
            }
        };

        var result = await controller.Delete(authorId, cancellationToken);

        Assert.That(result, Is.TypeOf<NoContentResult>());
        authService.Verify(service => service.EnsureCurrentUserIsActive(email, cancellationToken), Times.Once);
        authorService.Verify(service => service.Delete(authorId, authorId, cancellationToken), Times.Once);
    }

    [Test]
    public async Task Delete_ShouldReturnUnauthorized_WhenAuthorIdClaimIsMissing()
    {
        var authorService = new Mock<IAuthorService>();
        var authService = new Mock<IAuthService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var authorId = Guid.NewGuid();
        var controller = new AuthorsController(authorService.Object, authService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreateUser(null, "manuel@portfolio.local")
                }
            }
        };

        authService
            .Setup(service => service.EnsureCurrentUserIsActive("manuel@portfolio.local", cancellationToken))
            .Returns(Task.CompletedTask);

        var result = await controller.Delete(authorId, cancellationToken);

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
        authService.Verify(service => service.EnsureCurrentUserIsActive("manuel@portfolio.local", cancellationToken), Times.Once);
        authorService.Verify(service => service.Delete(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
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
