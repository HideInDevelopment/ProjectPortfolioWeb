namespace PortfolioWeb.Application.Contract.Exceptions.Auth;

public class InactiveUserException : Exception
{
    public InactiveUserException()
        : base("The user account is inactive.")
    {
    }
}
