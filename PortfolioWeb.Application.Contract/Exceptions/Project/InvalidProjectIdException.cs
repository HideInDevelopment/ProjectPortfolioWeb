namespace PortfolioWeb.Application.Contract.Exceptions.Project;

public class InvalidProjectIdException : Exception
{
    public InvalidProjectIdException()
        : base("The provided project id is not valid.")
    {
    }
}
