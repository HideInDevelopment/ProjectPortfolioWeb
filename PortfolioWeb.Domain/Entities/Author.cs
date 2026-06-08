namespace PortfolioWeb.Domain.Entities;

public class Author
{
    private readonly List<Project> _projects;

    public Author(Guid id, string name)
    {
        Id = id;
        Name = name;
        _projects = [];
    }

    public Guid Id { get; set; }

    public string Name { get; set; }

    public IReadOnlyList<Project> Projects => _projects;

    public void AddProject(Project project)
    {
        _projects.Add(project);
    }
}
