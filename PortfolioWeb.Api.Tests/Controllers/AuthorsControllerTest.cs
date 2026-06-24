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
        var cancellationToken = new CancellationTokenSource().Token;
        var authors = new List<AuthorDTO>
        {
            new() { Id = Guid.NewGuid(), Name = "Manuel" }
        };

        authorService
            .Setup(service => service.GetAll(cancellationToken))
            .ReturnsAsync(authors);

        var controller = new AuthorsController(authorService.Object);

        var result = await controller.GetAll(cancellationToken);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(authors));
        authorService.Verify(service => service.GetAll(cancellationToken), Times.Once);
    }

    [Test]
    public async Task GetById_ShouldReturnOkWithAuthor()
    {
        var authorService = new Mock<IAuthorService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var authorId = Guid.NewGuid();
        var author = new AuthorDTO { Id = authorId, Name = "Manuel" };

        authorService
            .Setup(service => service.GetById(authorId, cancellationToken))
            .ReturnsAsync(author);

        var controller = new AuthorsController(authorService.Object);

        var result = await controller.GetById(authorId, cancellationToken);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(author));
        authorService.Verify(service => service.GetById(authorId, cancellationToken), Times.Once);
    }

    [Test]
    public async Task Update_ShouldReturnOkWithUpdatedAuthor()
    {
        var authorService = new Mock<IAuthorService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var authorId = Guid.NewGuid();
        var persistedAuthorDto = new PersistAuthorDTO { Name = "Updated" };
        var updatedAuthor = new AuthorDTO { Id = authorId, Name = "Updated" };

        authorService
            .Setup(service => service.Update(authorId, persistedAuthorDto, authorId, cancellationToken))
            .ReturnsAsync(updatedAuthor);

        var controller = new AuthorsController(authorService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreateUser(authorId)
                }
            }
        };

        var result = await controller.Update(authorId, persistedAuthorDto, cancellationToken);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(updatedAuthor));
        authorService.Verify(service => service.Update(authorId, persistedAuthorDto, authorId, cancellationToken), Times.Once);
    }

    [Test]
    public async Task Update_ShouldReturnUnauthorized_WhenAuthorIdClaimIsMissing()
    {
        var authorService = new Mock<IAuthorService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var authorId = Guid.NewGuid();
        var persistedAuthorDto = new PersistAuthorDTO { Name = "Updated" };
        var controller = new AuthorsController(authorService.Object)
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
        authorService.Verify(service => service.Update(It.IsAny<Guid>(), It.IsAny<PersistAuthorDTO>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Delete_ShouldReturnNoContent()
    {
        var authorService = new Mock<IAuthorService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var authorId = Guid.NewGuid();

        authorService
            .Setup(service => service.Delete(authorId, authorId, cancellationToken))
            .Returns(Task.CompletedTask);

        var controller = new AuthorsController(authorService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreateUser(authorId)
                }
            }
        };

        var result = await controller.Delete(authorId, cancellationToken);

        Assert.That(result, Is.TypeOf<NoContentResult>());
        authorService.Verify(service => service.Delete(authorId, authorId, cancellationToken), Times.Once);
    }

    [Test]
    public async Task Delete_ShouldReturnUnauthorized_WhenAuthorIdClaimIsMissing()
    {
        var authorService = new Mock<IAuthorService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var authorId = Guid.NewGuid();
        var controller = new AuthorsController(authorService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            }
        };

        var result = await controller.Delete(authorId, cancellationToken);

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
        authorService.Verify(service => service.Delete(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static ClaimsPrincipal CreateUser(Guid authorId)
    {
        return new ClaimsPrincipal(
            new ClaimsIdentity(
            [
                new Claim("authorId", authorId.ToString())
            ],
            authenticationType: "Test"));
    }
}
