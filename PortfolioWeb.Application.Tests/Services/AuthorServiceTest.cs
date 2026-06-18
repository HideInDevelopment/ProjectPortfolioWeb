using Microsoft.Extensions.Logging;
using Moq;
using PortfolioWeb.Application.Contract.DTOs;
using PortfolioWeb.Application.Contract.Exceptions.Author;
using PortfolioWeb.Application.Services;
using PortfolioWeb.Core.Contracts.Repositories;
using PortfolioWeb.Domain.Entities;

namespace PortfolioWeb.Application.Tests.Services;

public class AuthorServiceTest
{
    private Mock<IAuthorRepository> _authorRepositoryMock = null!;
    private Mock<ILogger<AuthorService>> _loggerMock = null!;
    private AuthorService _authorService = null!;

    [SetUp]
    public void SetUp()
    {
        _authorRepositoryMock = new Mock<IAuthorRepository>();
        _loggerMock = new Mock<ILogger<AuthorService>>();

        _authorService = new AuthorService(
            _authorRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task GetById_ShouldReturnAuthorDto_WhenAuthorExists()
    {
        var authorId = Guid.NewGuid();
        var author = CreateAuthor(authorId, "Manuel");

        _authorRepositoryMock
            .Setup(x => x.GetById(authorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(author);

        var result = await _authorService.GetById(authorId);

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(authorId));
            Assert.That(result.Name, Is.EqualTo("Manuel"));
            Assert.That(result.Projects, Is.Empty);
        });
    }

    [Test]
    public void GetById_ShouldThrowInvalidAuthorIdException_WhenIdIsEmpty()
    {
        var exception = Assert.ThrowsAsync<InvalidAuthorIdException>(
            async () => await _authorService.GetById(Guid.Empty));

        Assert.That(exception!.Message, Is.EqualTo("The provided author id is not valid."));
        _authorRepositoryMock.Verify(
            x => x.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public void GetById_ShouldThrowAuthorNotFoundException_WhenAuthorDoesNotExist()
    {
        var authorId = Guid.NewGuid();

        _authorRepositoryMock
            .Setup(x => x.GetById(authorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Author?)null);

        var exception = Assert.ThrowsAsync<AuthorNotFoundException>(
            async () => await _authorService.GetById(authorId));

        Assert.That(exception!.Message, Is.EqualTo($"The author with id '{authorId}' was not found."));
    }

    [Test]
    public async Task GetAll_ShouldReturnMappedAuthors_WhenRepositoryReturnsAuthors()
    {
        var authors = new List<Author>
        {
            CreateAuthor(Guid.NewGuid(), "Manuel"),
            CreateAuthor(Guid.NewGuid(), "Maria")
        };

        _authorRepositoryMock
            .Setup(x => x.GetAll(It.IsAny<CancellationToken>()))
            .ReturnsAsync(authors);

        var result = await _authorService.GetAll();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(result[0].Id, Is.EqualTo(authors[0].Id));
            Assert.That(result[0].Name, Is.EqualTo(authors[0].Name));
            Assert.That(result[1].Id, Is.EqualTo(authors[1].Id));
            Assert.That(result[1].Name, Is.EqualTo(authors[1].Name));
        });
    }

    [Test]
    public async Task Create_ShouldMapPersistAuthorDtoAndReturnCreatedAuthorDto()
    {
        var authorDto = new PersistAuthorDTO
        {
            Name = "Manuel"
        };

        Author? createdAuthorArgument = null;
        var persistedAuthor = CreateAuthor(Guid.NewGuid(), "Manuel");

        _authorRepositoryMock
            .Setup(x => x.Create(It.IsAny<Author>(), It.IsAny<CancellationToken>()))
            .Callback<Author, CancellationToken>((author, _) => createdAuthorArgument = author)
            .ReturnsAsync(persistedAuthor);

        var result = await _authorService.Create(authorDto);

        Assert.That(createdAuthorArgument, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(createdAuthorArgument!.Name, Is.EqualTo("Manuel"));
            Assert.That(createdAuthorArgument.Id, Is.EqualTo(Guid.Empty));
            Assert.That(result.Id, Is.EqualTo(persistedAuthor.Id));
            Assert.That(result.Name, Is.EqualTo("Manuel"));
        });
    }

    [Test]
    public async Task Update_ShouldMapPersistAuthorDtoAndReturnUpdatedAuthorDto_WhenAuthorExists()
    {
        var authorId = Guid.NewGuid();
        var authorDto = new PersistAuthorDTO
        {
            Name = "Updated Name"
        };

        Author? updatedAuthorArgument = null;
        var persistedAuthor = CreateAuthor(authorId, "Updated Name");

        _authorRepositoryMock
            .Setup(x => x.Update(It.IsAny<Author>(), It.IsAny<CancellationToken>()))
            .Callback<Author, CancellationToken>((author, _) => updatedAuthorArgument = author)
            .ReturnsAsync(persistedAuthor);

        var result = await _authorService.Update(authorId, authorDto);

        Assert.That(updatedAuthorArgument, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(updatedAuthorArgument!.Id, Is.EqualTo(authorId));
            Assert.That(updatedAuthorArgument.Name, Is.EqualTo("Updated Name"));
            Assert.That(result.Id, Is.EqualTo(authorId));
            Assert.That(result.Name, Is.EqualTo("Updated Name"));
        });
    }

    [Test]
    public void Update_ShouldThrowInvalidAuthorIdException_WhenIdIsEmpty()
    {
        var authorDto = new PersistAuthorDTO
        {
            Name = "Updated Name"
        };

        var exception = Assert.ThrowsAsync<InvalidAuthorIdException>(
            async () => await _authorService.Update(Guid.Empty, authorDto));

        Assert.That(exception!.Message, Is.EqualTo("The provided author id is not valid."));
        _authorRepositoryMock.Verify(
            x => x.Update(It.IsAny<Author>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public void Update_ShouldThrowAuthorNotFoundException_WhenAuthorDoesNotExist()
    {
        var authorId = Guid.NewGuid();
        var authorDto = new PersistAuthorDTO
        {
            Name = "Updated Name"
        };

        _authorRepositoryMock
            .Setup(x => x.Update(It.IsAny<Author>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Author?)null);

        var exception = Assert.ThrowsAsync<AuthorNotFoundException>(
            async () => await _authorService.Update(authorId, authorDto));

        Assert.That(exception!.Message, Is.EqualTo($"The author with id '{authorId}' was not found."));
    }

    [Test]
    public void Delete_ShouldThrowInvalidAuthorIdException_WhenIdIsEmpty()
    {
        var exception = Assert.ThrowsAsync<InvalidAuthorIdException>(
            async () => await _authorService.Delete(Guid.Empty));

        Assert.That(exception!.Message, Is.EqualTo("The provided author id is not valid."));
        _authorRepositoryMock.Verify(
            x => x.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _authorRepositoryMock.Verify(
            x => x.Delete(It.IsAny<Author>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public void Delete_ShouldThrowAuthorNotFoundException_WhenAuthorDoesNotExist()
    {
        var authorId = Guid.NewGuid();

        _authorRepositoryMock
            .Setup(x => x.GetById(authorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Author?)null);

        var exception = Assert.ThrowsAsync<AuthorNotFoundException>(
            async () => await _authorService.Delete(authorId));

        Assert.That(exception!.Message, Is.EqualTo($"The author with id '{authorId}' was not found."));
        _authorRepositoryMock.Verify(
            x => x.Delete(It.IsAny<Author>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Delete_ShouldDeleteAuthorSuccessfully()
    {
        var authorId = Guid.NewGuid();
        var author = CreateAuthor(authorId, "Manuel");

        _authorRepositoryMock
            .Setup(x => x.GetById(authorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(author);

        _authorRepositoryMock
            .Setup(x => x.Delete(author, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _authorService.Delete(authorId);

        _authorRepositoryMock.Verify(
            x => x.Delete(author, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static Author CreateAuthor(Guid id, string name)
    {
        return new Author(name)
        {
            Id = id
        };
    }
}
