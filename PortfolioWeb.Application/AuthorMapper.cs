using PortfolioWeb.Application.Contract.DTOs;
using PortfolioWeb.Domain.Entities;

namespace PortfolioWeb.Application;

public static class AuthorMapper
{
    public static AuthorDTO MapToDTO(Author author)
    {
        return new AuthorDTO
        {
            Id = author.Id,
            Name = author.Name,
            Projects = author.Projects
                .Select(ProjectMapper.MapToDTO)
                .ToList()
        };
    }

    public static Author MapToEntity(AuthorDTO authorDto)
    {
        var author = new Author(authorDto.Name);
        var projects = authorDto.Projects
            .Select(ProjectMapper.MapToEntity)
            .ToList();

        foreach (var project in projects)
        {
            author.AddProject(project);
        }

        return author;
    }

    public static Author MapToEntity(CreateAuthorDTO authorDto)
    {
        var author = new Author(authorDto.Name);
        var projects = authorDto.Projects
            .Select(ProjectMapper.MapToEntity)
            .ToList();

        foreach (var project in projects)
        {
            author.AddProject(project);
        }

        return author;
    }
}
