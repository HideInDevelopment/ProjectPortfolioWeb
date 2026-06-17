using Microsoft.AspNetCore.Mvc;
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
    public async Task Create_ShouldReturnCreatedAtActionWithCreatedAuthor()
    {
        var authorService = new Mock<IAuthorService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var persistedAuthorDto = new PersistAuthorDTO { Name = "Manuel" };
        var createdAuthor = new AuthorDTO { Id = Guid.NewGuid(), Name = "Manuel" };

        authorService
            .Setup(service => service.Create(persistedAuthorDto, cancellationToken))
            .ReturnsAsync(createdAuthor);

        var controller = new AuthorsController(authorService.Object);

        var result = await controller.Create(persistedAuthorDto, cancellationToken);
        var createdAtActionResult = result.Result as CreatedAtActionResult;

        Assert.Multiple(() =>
        {
            Assert.That(createdAtActionResult, Is.Not.Null);
            Assert.That(createdAtActionResult!.ActionName, Is.EqualTo(nameof(AuthorsController.GetById)));
            Assert.That(createdAtActionResult.RouteValues!["id"], Is.EqualTo(createdAuthor.Id));
            Assert.That(createdAtActionResult.Value, Is.SameAs(createdAuthor));
        });

        authorService.Verify(service => service.Create(persistedAuthorDto, cancellationToken), Times.Once);
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
            .Setup(service => service.Update(authorId, persistedAuthorDto, cancellationToken))
            .ReturnsAsync(updatedAuthor);

        var controller = new AuthorsController(authorService.Object);

        var result = await controller.Update(authorId, persistedAuthorDto, cancellationToken);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(updatedAuthor));
        authorService.Verify(service => service.Update(authorId, persistedAuthorDto, cancellationToken), Times.Once);
    }

    [Test]
    public async Task Delete_ShouldReturnNoContent()
    {
        var authorService = new Mock<IAuthorService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var authorId = Guid.NewGuid();

        authorService
            .Setup(service => service.Delete(authorId, cancellationToken))
            .ReturnsAsync(true);

        var controller = new AuthorsController(authorService.Object);

        var result = await controller.Delete(authorId, cancellationToken);

        Assert.That(result, Is.TypeOf<NoContentResult>());
        authorService.Verify(service => service.Delete(authorId, cancellationToken), Times.Once);
    }
}
