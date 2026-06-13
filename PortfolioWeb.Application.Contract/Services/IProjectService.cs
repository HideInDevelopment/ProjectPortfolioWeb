using PortfolioWeb.Application.Contract.DTOs;

namespace PortfolioWeb.Application.Contract.Services;

public interface IProjectService
{
    Task<ProjectDTO?> GetById(Guid id, CancellationToken cancellationToken = default);

    Task<List<ProjectDTO>> GetAll(CancellationToken cancellationToken = default);

    Task<ProjectDTO> Create(CreateProjectDTO projectDto, CancellationToken cancellationToken = default);

    Task<ProjectDTO?> Update(Guid id, UpdateProjectDTO projectDto, CancellationToken cancellationToken = default);

    Task<bool> Delete(Guid id, CancellationToken cancellationToken = default);
}
