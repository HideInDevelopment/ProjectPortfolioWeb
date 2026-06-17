using PortfolioWeb.Application.Contract.DTOs;
using PortfolioWeb.Application.Mappers;
using PortfolioWeb.Domain.Entities;

namespace PortfolioWeb.Application.Tests.Mappers;

public class AuthorMapperTest
{
    [Test]
    public void MapToDTO_ShouldMapAuthorAndProjectsCorrectly()
    {
        var authorId = Guid.NewGuid();
        var author = new Author("Manuel")
        {
            Id = authorId
        };

        var project = new Project(
            "PortfolioWeb",
            "Personal portfolio website.",
            new DateTimeOffset(2026, 06, 17, 0, 0, 0, TimeSpan.Zero),
            1,
            authorId,
            true)
        {
            Id = Guid.NewGuid()
        };

        author.AddProject(project);

        var result = AuthorMapper.MapToDTO(author);

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(authorId));
            Assert.That(result.Name, Is.EqualTo("Manuel"));
            Assert.That(result.Projects, Has.Count.EqualTo(1));
            Assert.That(result.Projects[0].Id, Is.EqualTo(project.Id));
            Assert.That(result.Projects[0].Title, Is.EqualTo(project.Title));
            Assert.That(result.Projects[0].Description, Is.EqualTo(project.Description));
            Assert.That(result.Projects[0].ReleaseDate, Is.EqualTo(project.ReleaseDate));
            Assert.That(result.Projects[0].Version, Is.EqualTo(project.Version));
            Assert.That(result.Projects[0].AuthorId, Is.EqualTo(project.AuthorId));
            Assert.That(result.Projects[0].IsInDevelopment, Is.EqualTo(project.IsInDevelopment));
        });
    }

    [Test]
    public void MapToEntity_ShouldMapPersistAuthorDtoCorrectly()
    {
        var authorDto = new PersistAuthorDTO
        {
            Name = "Manuel"
        };

        var result = AuthorMapper.MapToEntity(authorDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(Guid.Empty));
            Assert.That(result.Name, Is.EqualTo("Manuel"));
            Assert.That(result.Projects, Is.Empty);
        });
    }
}
