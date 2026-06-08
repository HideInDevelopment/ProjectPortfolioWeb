namespace PortfolioWeb.Application.Contract.DTOs;

public class CreateProjectDTO
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset ReleaseDate { get; set; }

    public int Version { get; set; }

    public Guid AuthorId { get; set; }

    public bool IsInDevelopment { get; set; }
}
