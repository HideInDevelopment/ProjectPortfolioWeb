using PortfolioWeb.Application.Contract.Dtos;

namespace PortfolioWeb.Application.Contract.Services;

public interface IProjectService
{
    Task<ProjectDto> GetById(Guid id, CancellationToken cancellationToken = default);

    Task<List<ProjectDto>> GetAll(CancellationToken cancellationToken = default);

    Task<ProjectDto> Create(CreateProjectDto projectDto, Guid currentAuthorId, CancellationToken cancellationToken = default);

    Task<ProjectDto> Update(Guid id, UpdateProjectDto projectDto, Guid currentAuthorId, CancellationToken cancellationToken = default);

    Task Delete(Guid id, Guid currentAuthorId, CancellationToken cancellationToken = default);
}

