namespace PortfolioWeb.Core.Contracts.Exceptions;

public class InfrastructureQueryException : InfrastructureException
{
    public InfrastructureQueryException(string message) : base(message)
    {
    }

    public InfrastructureQueryException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
