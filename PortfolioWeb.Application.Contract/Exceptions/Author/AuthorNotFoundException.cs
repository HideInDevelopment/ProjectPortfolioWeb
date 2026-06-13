namespace PortfolioWeb.Application.Contract.Exceptions.Author;

public class AuthorNotFoundException : Exception
{
    public AuthorNotFoundException(Guid authorId)
        : base($"The author with id '{authorId}' was not found.")
    {
    }
}
