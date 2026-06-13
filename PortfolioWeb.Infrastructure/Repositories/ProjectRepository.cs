using Microsoft.EntityFrameworkCore;
using PortfolioWeb.Domain.Contract.Repositories;
using PortfolioWeb.Domain.Entities;
using PortfolioWeb.Infrastructure.Persistence;

namespace PortfolioWeb.Infrastructure.Repositories;

public class ProjectRepository(PortfolioWebDbContext dbContext) : IProjectRepository
{
    public async Task<Project?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Projects
            .FirstOrDefaultAsync(project => project.Id == id, cancellationToken);
    }

    public async Task<List<Project>> GetAll(CancellationToken cancellationToken = default)
    {
        return await dbContext.Projects.ToListAsync(cancellationToken);
    }

    public async Task<Project> Create(Project project, CancellationToken cancellationToken = default)
    {
        await dbContext.Projects.AddAsync(project, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return project;
    }

    public async Task<Project?> Update(Project project, CancellationToken cancellationToken = default)
    {
        var existingProject = await dbContext.Projects
            .FirstOrDefaultAsync(x => x.Id == project.Id, cancellationToken);

        if (existingProject is null)
        {
            return null;
        }

        dbContext.Entry(existingProject).CurrentValues.SetValues(project);
        await dbContext.SaveChangesAsync(cancellationToken);

        return existingProject;
    }

    public async Task<bool> Delete(Project project, CancellationToken cancellationToken = default)
    {
        try
        {
            dbContext.Projects.Remove(project);
            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
