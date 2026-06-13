namespace PortfolioWeb.Application.Contract.Exceptions.Author;

public class InvalidAuthorIdException : Exception
{
    public InvalidAuthorIdException()
        : base("The provided author id is not valid.")
    {
    }
}
