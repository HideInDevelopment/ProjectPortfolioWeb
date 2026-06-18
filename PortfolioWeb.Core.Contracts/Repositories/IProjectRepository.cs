using PortfolioWeb.Domain.Entities;

namespace PortfolioWeb.Core.Contracts.Repositories;

public interface IProjectRepository
{
    Task<Project?> GetById(Guid id, CancellationToken cancellationToken = default);

    Task<List<Project>> GetAll(CancellationToken cancellationToken = default);

    Task<Project> Create(Project project, CancellationToken cancellationToken = default);

    Task<Project?> Update(Project project, CancellationToken cancellationToken = default);

    Task Delete(Project project, CancellationToken cancellationToken = default);
}
