using PortfolioWeb.Application.Contract.Dtos;
using PortfolioWeb.Domain.Entities;

namespace PortfolioWeb.Application.Mappers;

public static class ProjectMapper
{
    public static ProjectDto MapToDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            Title = project.Title,
            Description = project.Description,
            ReleaseDate = project.ReleaseDate,
            Version = project.Version,
            AuthorId = project.AuthorId,
            IsInDevelopment = project.IsInDevelopment
        };
    }

    public static Project MapToEntity(CreateProjectDto projectDto, Guid authorId)
    {
        return new Project(
            projectDto.Title,
            projectDto.Description,
            projectDto.ReleaseDate,
            projectDto.Version,
            authorId,
            projectDto.IsInDevelopment);
    }
}

