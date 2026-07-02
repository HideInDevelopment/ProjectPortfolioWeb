using Microsoft.EntityFrameworkCore;
using PortfolioWeb.Core.Contracts.Exceptions;
using PortfolioWeb.Core.Contracts.Repositories;
using PortfolioWeb.Domain.Entities;
using PortfolioWeb.Infrastructure.Exceptions;
using PortfolioWeb.Infrastructure.Persistence;

namespace PortfolioWeb.Infrastructure.Repositories;

public class AuthorRepository(PortfolioWebDbContext dbContext) : IAuthorRepository
{
    public async Task<Author?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.Authors
                .Include(author => author.Projects)
                .FirstOrDefaultAsync(author => author.Id == id, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (ExceptionClassifier.IsConnectionException(exception))
        {
            throw new InfrastructureConnectionException("An error occurred while connecting to the database to retrieve the author.", exception);
        }
        catch (Exception exception) when (ExceptionClassifier.IsQueryException(exception))
        {
            throw new InfrastructureQueryException("An error occurred while retrieving the author from the database.", exception);
        }
    }

    public async Task<List<Author>> GetAll(CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.Authors
                .Include(author => author.Projects)
                .ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (ExceptionClassifier.IsConnectionException(exception))
        {
            throw new InfrastructureConnectionException("An error occurred while connecting to the database to retrieve the authors.", exception);
        }
        catch (Exception exception) when (ExceptionClassifier.IsQueryException(exception))
        {
            throw new InfrastructureQueryException("An error occurred while retrieving the authors from the database.", exception);
        }
    }

    public async Task<Author?> Update(Author author, CancellationToken cancellationToken = default)
    {
        Author? existingAuthor;

        try
        {
            existingAuthor = await dbContext.Authors
                .Include(x => x.Projects)
                .FirstOrDefaultAsync(x => x.Id == author.Id, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (ExceptionClassifier.IsConnectionException(exception))
        {
            throw new InfrastructureConnectionException("An error occurred while connecting to the database to update the author.", exception);
        }
        catch (Exception exception) when (ExceptionClassifier.IsQueryException(exception))
        {
            throw new InfrastructureQueryException("An error occurred while retrieving the author to update it.", exception);
        }

        if (existingAuthor is null)
        {
            return null;
        }

        try
        {
            existingAuthor.Name = author.Name;
            await dbContext.SaveChangesAsync(cancellationToken);

            return existingAuthor;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (ExceptionClassifier.IsConnectionException(exception))
        {
            throw new InfrastructureConnectionException("An error occurred while connecting to the database to update the author.", exception);
        }
        catch (Exception exception) when (ExceptionClassifier.IsPersistenceException(exception))
        {
            throw new InfrastructurePersistenceException("An error occurred while saving the updated author in the database.", exception);
        }
    }

    public async Task Delete(Author author, CancellationToken cancellationToken = default)
    {
        try
        {
            dbContext.Authors.Remove(author);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (ExceptionClassifier.IsConnectionException(exception))
        {
            throw new InfrastructureConnectionException("An error occurred while connecting to the database to delete the author.", exception);
        }
        catch (Exception exception) when (ExceptionClassifier.IsPersistenceException(exception))
        {
            throw new InfrastructurePersistenceException("An error occurred while deleting the author from the database.", exception);
        }
    }
}
