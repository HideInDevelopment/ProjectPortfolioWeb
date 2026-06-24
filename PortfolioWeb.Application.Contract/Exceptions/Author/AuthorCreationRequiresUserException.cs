namespace PortfolioWeb.Application.Contract.Exceptions.Author;

public class AuthorCreationRequiresUserException : Exception
{
    public AuthorCreationRequiresUserException()
        : base("Authors must be created through user registration.")
    {
    }
}
