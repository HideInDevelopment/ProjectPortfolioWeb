using PortfolioWeb.Application.Contract.DTOs;
using PortfolioWeb.Domain.Entities;

namespace PortfolioWeb.Application.Mappers;

public static class ProjectMapper
{
    public static ProjectDTO MapToDTO(Project project)
    {
        return new ProjectDTO
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

    public static Project MapToEntity(ProjectDTO projectDto)
    {
        return new Project(
            projectDto.Title,
            projectDto.Description,
            projectDto.ReleaseDate,
            projectDto.Version,
            projectDto.AuthorId,
            projectDto.IsInDevelopment);
    }

    public static Project MapToEntity(CreateProjectDTO projectDto)
    {
        return new Project(
            projectDto.Title,
            projectDto.Description,
            projectDto.ReleaseDate,
            projectDto.Version,
            projectDto.AuthorId,
            projectDto.IsInDevelopment);
    }
}
