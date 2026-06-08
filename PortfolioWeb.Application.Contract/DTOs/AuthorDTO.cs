namespace PortfolioWeb.Application.Contract.DTOs;

public class AuthorDTO
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<ProjectDTO> Projects { get; set; } = [];
}
