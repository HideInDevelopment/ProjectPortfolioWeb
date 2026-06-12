using PortfolioWeb.Application.Contract.DTOs;
using PortfolioWeb.Domain.Entities;

namespace PortfolioWeb.Application;

public static class AuthorMapper
{
    public static AuthorDTO MapToDTO(Author author) =>
        new()
        {
            Id = author.Id,
            Name = author.Name,
            Projects = author.Projects
                .Select(ProjectMapper.MapToDTO)
                .ToList()
        };

    public static Author MapToEntity(PersistAuthorDTO authorDto) => new(authorDto.Name);
}
