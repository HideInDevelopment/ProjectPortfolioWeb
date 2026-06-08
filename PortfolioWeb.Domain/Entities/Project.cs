namespace PortfolioWeb.Domain.Entities;

public class Project
{
    public Guid Id { get; set; }

    public required string Title { get; set; }

    public required string Description { get; set; }

    public DateTimeOffset ReleaseDate { get; set; }

    public int Version { get; set; }

    public Guid AuthorId { get; set; }
}
