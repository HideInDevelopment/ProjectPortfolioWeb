namespace PortfolioWeb.Domain.Entities;

public class Project
{
    private Project()
    {
        Title = string.Empty;
        Description = string.Empty;
    }

    public Project(
        string title,
        string description,
        DateTimeOffset releaseDate,
        int version,
        Guid authorId,
        bool isInDevelopment)
    {
        Title = title;
        Description = description;
        ReleaseDate = releaseDate;
        Version = version;
        AuthorId = authorId;
        IsInDevelopment = isInDevelopment;
    }

    public Guid Id { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public DateTimeOffset ReleaseDate { get; set; }

    public int Version { get; set; }

    public Guid AuthorId { get; set; }

    public bool IsInDevelopment { get; set; }
}
