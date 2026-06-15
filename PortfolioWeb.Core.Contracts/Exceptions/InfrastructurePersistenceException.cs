namespace PortfolioWeb.Core.Contracts.Exceptions;

public class InfrastructurePersistenceException : InfrastructureException
{
    public InfrastructurePersistenceException()
    {
    }

    public InfrastructurePersistenceException(string message) : base(message)
    {
    }

    public InfrastructurePersistenceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
