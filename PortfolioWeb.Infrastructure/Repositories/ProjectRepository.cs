using Microsoft.EntityFrameworkCore;
using PortfolioWeb.Core.Contracts.Exceptions;
using PortfolioWeb.Core.Contracts.Repositories;
using PortfolioWeb.Domain.Entities;
using PortfolioWeb.Infrastructure.Exceptions;
using PortfolioWeb.Infrastructure.Persistence;

namespace PortfolioWeb.Infrastructure.Repositories;

public class ProjectRepository(PortfolioWebDbContext dbContext) : IProjectRepository
{
    public async Task<Project?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.Projects
                .FirstOrDefaultAsync(project => project.Id == id, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (ExceptionClassifier.IsConnectionException(exception))
        {
            throw new InfrastructureConnectionException("An error occurred while connecting to the database to retrieve the project.", exception);
        }
        catch (Exception exception) when (ExceptionClassifier.IsQueryException(exception))
        {
            throw new InfrastructureQueryException("An error occurred while retrieving the project from the database.", exception);
        }
    }

    public async Task<List<Project>> GetAll(CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.Projects.ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (ExceptionClassifier.IsConnectionException(exception))
        {
            throw new InfrastructureConnectionException("An error occurred while connecting to the database to retrieve the projects.", exception);
        }
        catch (Exception exception) when (ExceptionClassifier.IsQueryException(exception))
        {
            throw new InfrastructureQueryException("An error occurred while retrieving the projects from the database.", exception);
        }
    }

    public async Task<Project> Create(Project project, CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.Projects.AddAsync(project, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return project;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (ExceptionClassifier.IsConnectionException(exception))
        {
            throw new InfrastructureConnectionException("An error occurred while connecting to the database to create the project.", exception);
        }
        catch (Exception exception) when (ExceptionClassifier.IsPersistenceException(exception))
        {
            throw new InfrastructurePersistenceException("An error occurred while saving the project in the database.", exception);
        }
    }

    public async Task<Project?> Update(Project project, CancellationToken cancellationToken = default)
    {
        Project? existingProject;

        try
        {
            existingProject = await dbContext.Projects
                .FirstOrDefaultAsync(x => x.Id == project.Id, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (ExceptionClassifier.IsConnectionException(exception))
        {
            throw new InfrastructureConnectionException("An error occurred while connecting to the database to update the project.", exception);
        }
        catch (Exception exception) when (ExceptionClassifier.IsQueryException(exception))
        {
            throw new InfrastructureQueryException("An error occurred while retrieving the project to update it.", exception);
        }

        if (existingProject is null)
        {
            return null;
        }

        try
        {
            dbContext.Entry(existingProject).CurrentValues.SetValues(project);
            await dbContext.SaveChangesAsync(cancellationToken);

            return existingProject;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (ExceptionClassifier.IsConnectionException(exception))
        {
            throw new InfrastructureConnectionException("An error occurred while connecting to the database to update the project.", exception);
        }
        catch (Exception exception) when (ExceptionClassifier.IsPersistenceException(exception))
        {
            throw new InfrastructurePersistenceException("An error occurred while saving the updated project in the database.", exception);
        }
    }

    public async Task Delete(Project project, CancellationToken cancellationToken = default)
    {
        try
        {
            dbContext.Projects.Remove(project);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (ExceptionClassifier.IsConnectionException(exception))
        {
            throw new InfrastructureConnectionException("An error occurred while connecting to the database to delete the project.", exception);
        }
        catch (Exception exception) when (ExceptionClassifier.IsPersistenceException(exception))
        {
            throw new InfrastructurePersistenceException("An error occurred while deleting the project from the database.", exception);
        }
    }
}
