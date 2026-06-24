namespace PortfolioWeb.Application.Contract.Exceptions.Auth;

public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException()
        : base("The provided credentials are not valid.")
    {
    }
}
