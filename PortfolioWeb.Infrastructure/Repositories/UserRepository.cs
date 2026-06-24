using Microsoft.EntityFrameworkCore;
using PortfolioWeb.Core.Contracts.Exceptions;
using PortfolioWeb.Core.Contracts.Repositories;
using PortfolioWeb.Domain.Entities;
using PortfolioWeb.Infrastructure.Exceptions;
using PortfolioWeb.Infrastructure.Persistence;

namespace PortfolioWeb.Infrastructure.Repositories;

public class UserRepository(PortfolioWebDbContext dbContext) : IUserRepository
{
    public async Task<User?> GetByEmail(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.Users
                .Include(user => user.Author)
                .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (ExceptionClassifier.IsConnectionException(exception))
        {
            throw new InfrastructureConnectionException("An error occurred while connecting to the database to retrieve the user.", exception);
        }
        catch (Exception exception) when (ExceptionClassifier.IsQueryException(exception))
        {
            throw new InfrastructureQueryException("An error occurred while retrieving the user from the database.", exception);
        }
    }

    public async Task<User> Create(User user, CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.Users.AddAsync(user, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return user;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (ExceptionClassifier.IsConnectionException(exception))
        {
            throw new InfrastructureConnectionException("An error occurred while connecting to the database to create the user.", exception);
        }
        catch (Exception exception) when (ExceptionClassifier.IsPersistenceException(exception))
        {
            throw new InfrastructurePersistenceException("An error occurred while saving the user in the database.", exception);
        }
    }
}
