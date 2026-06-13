namespace PortfolioWeb.Application.Contract.Exceptions.Project;

public class ProjectNotFoundException : Exception
{
    public ProjectNotFoundException(Guid projectId)
        : base($"The project with id '{projectId}' was not found.")
    {
    }
}
