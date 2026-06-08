using PortfolioWeb.Application.Contract.DTOs;
using PortfolioWeb.Application.Contract.Services;
using PortfolioWeb.Domain.Contract.Repositories;

namespace PortfolioWeb.Application.Services;

public class ProjectService(IProjectRepository projectRepository) : IProjectService
{
    public async Task<ProjectDTO?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await projectRepository.GetById(id, cancellationToken);

        return project is null ? null : ProjectMapper.MapToDTO(project);
    }

    public async Task<List<ProjectDTO>> GetAll(CancellationToken cancellationToken = default)
    {
        var projects = await projectRepository.GetAll(cancellationToken);

        return projects
            .Select(ProjectMapper.MapToDTO)
            .ToList();
    }

    public async Task<ProjectDTO> Create(ProjectDTO projectDto, CancellationToken cancellationToken = default)
    {
        var project = ProjectMapper.MapToEntity(projectDto);
        var createdProject = await projectRepository.Create(project, cancellationToken);

        return ProjectMapper.MapToDTO(createdProject);
    }

    public async Task<ProjectDTO> Update(ProjectDTO projectDto, CancellationToken cancellationToken = default)
    {
        var project = ProjectMapper.MapToEntity(projectDto);
        var updatedProject = await projectRepository.Update(project, cancellationToken);

        return ProjectMapper.MapToDTO(updatedProject);
    }

    public async Task Delete(Guid id, CancellationToken cancellationToken = default)
    {
        await projectRepository.Delete(id, cancellationToken);
    }
}
