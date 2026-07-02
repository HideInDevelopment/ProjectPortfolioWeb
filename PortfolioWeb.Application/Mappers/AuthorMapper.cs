using PortfolioWeb.Application.Contract.Dtos;
using PortfolioWeb.Domain.Entities;

namespace PortfolioWeb.Application.Mappers;

public static class AuthorMapper
{
    public static AuthorDto MapToDto(Author author) =>
        new()
        {
            Id = author.Id,
            Name = author.Name,
            Projects = author.Projects
                .Select(ProjectMapper.MapToDto)
                .ToList()
        };

    public static Author MapToEntity(PersistAuthorDto authorDto) => new(authorDto.Name);
}

