namespace PortfolioWeb.Application.Contract.Dtos;

public class AuthorDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<ProjectDto> Projects { get; set; } = [];
}

