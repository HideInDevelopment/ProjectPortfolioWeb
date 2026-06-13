namespace PortfolioWeb.Application.Contract.Exceptions.Project;

public class ReferencedAuthorNotFoundException : Exception
{
    public ReferencedAuthorNotFoundException(Guid authorId)
        : base($"The referenced author with id '{authorId}' was not found.")
    {
    }
}
