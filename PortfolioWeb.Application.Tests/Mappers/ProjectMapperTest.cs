using PortfolioWeb.Application.Contract.Dtos;
using PortfolioWeb.Application.Mappers;
using PortfolioWeb.Domain.Entities;

namespace PortfolioWeb.Application.Tests.Mappers;

public class ProjectMapperTest
{
    [Test]
    public void MapToDto_ShouldMapProjectCorrectly()
    {
        var projectId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var releaseDate = new DateTimeOffset(2026, 06, 17, 0, 0, 0, TimeSpan.Zero);
        var project = new Project(
            "PortfolioWeb",
            "Personal portfolio website.",
            releaseDate,
            1,
            authorId,
            true)
        {
            Id = projectId
        };

        var result = ProjectMapper.MapToDto(project);

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(projectId));
            Assert.That(result.Title, Is.EqualTo(project.Title));
            Assert.That(result.Description, Is.EqualTo(project.Description));
            Assert.That(result.ReleaseDate, Is.EqualTo(project.ReleaseDate));
            Assert.That(result.Version, Is.EqualTo(project.Version));
            Assert.That(result.AuthorId, Is.EqualTo(project.AuthorId));
            Assert.That(result.IsInDevelopment, Is.EqualTo(project.IsInDevelopment));
        });
    }

    [Test]
    public void MapToEntity_ShouldMapCreateProjectDtoCorrectly()
    {
        var authorId = Guid.NewGuid();
        var releaseDate = new DateTimeOffset(2026, 07, 01, 0, 0, 0, TimeSpan.Zero);
        var projectDto = new CreateProjectDto
        {
            Title = "VetApp",
            Description = "Vet manager for schedule appointments.",
            ReleaseDate = releaseDate,
            Version = 2,
            IsInDevelopment = false
        };

        var result = ProjectMapper.MapToEntity(projectDto, authorId);

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(Guid.Empty));
            Assert.That(result.Title, Is.EqualTo(projectDto.Title));
            Assert.That(result.Description, Is.EqualTo(projectDto.Description));
            Assert.That(result.ReleaseDate, Is.EqualTo(projectDto.ReleaseDate));
            Assert.That(result.Version, Is.EqualTo(projectDto.Version));
            Assert.That(result.AuthorId, Is.EqualTo(authorId));
            Assert.That(result.IsInDevelopment, Is.EqualTo(projectDto.IsInDevelopment));
        });
    }
}

