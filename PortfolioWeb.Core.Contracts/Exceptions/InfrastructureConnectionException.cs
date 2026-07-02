namespace PortfolioWeb.Core.Contracts.Exceptions;

public class InfrastructureConnectionException : InfrastructureException
{
    public InfrastructureConnectionException(string message) : base(message)
    {
    }

    public InfrastructureConnectionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
