namespace PortfolioWeb.Domain.Entities;

public class Author
{
    private readonly List<Project> _projects;

    // Required by EF Core when materializing the entity from persistence.
    private Author()
    {
        Name = string.Empty;
        _projects = [];
        User = null!;
    }

    public Author(string name)
    {
        Name = name;
        _projects = [];
        User = null!;
    }

    public Guid Id { get; set; }

    public string Name { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; }

    public IReadOnlyList<Project> Projects => _projects;

    public void AddProject(Project project)
    {
        _projects.Add(project);
    }
}
