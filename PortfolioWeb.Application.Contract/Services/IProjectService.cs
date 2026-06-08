using PortfolioWeb.Application.Contract.DTOs;

namespace PortfolioWeb.Application.Contract.Services;

public interface IProjectService
{
    Task<ProjectDTO?> GetById(Guid id, CancellationToken cancellationToken = default);

    Task<List<ProjectDTO>> GetAll(CancellationToken cancellationToken = default);

    Task<ProjectDTO> Create(ProjectDTO projectDto, CancellationToken cancellationToken = default);

    Task<ProjectDTO> Update(ProjectDTO projectDto, CancellationToken cancellationToken = default);

    Task Delete(Guid id, CancellationToken cancellationToken = default);
}
