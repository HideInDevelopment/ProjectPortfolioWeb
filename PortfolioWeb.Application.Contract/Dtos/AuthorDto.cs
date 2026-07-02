namespace PortfolioWeb.Application.Contract.Dtos;

public class AuthorDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public List<ProjectDto> Projects { get; init; } = [];
}

