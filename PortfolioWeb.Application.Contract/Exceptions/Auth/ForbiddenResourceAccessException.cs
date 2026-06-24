namespace PortfolioWeb.Application.Contract.Exceptions.Auth;

public class ForbiddenResourceAccessException : Exception
{
    public ForbiddenResourceAccessException()
        : base("The current user is not allowed to access this resource.")
    {
    }
}
