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

    public async Task<ProjectDTO> Create(CreateProjectDTO projectDto, CancellationToken cancellationToken = default)
    {
        var project = ProjectMapper.MapToEntity(projectDto);
        var createdProject = await projectRepository.Create(project, cancellationToken);

        return ProjectMapper.MapToDTO(createdProject);
    }

    public async Task<ProjectDTO?> Update(Guid id, UpdateProjectDTO projectDto, CancellationToken cancellationToken = default)
    {
        var project = await projectRepository.GetById(id, cancellationToken);

        if (project is null)
        {
            return null;
        }

        project.Title = projectDto.Title;
        project.Description = projectDto.Description;
        project.Version = projectDto.Version;
        project.IsInDevelopment = projectDto.IsInDevelopment;

        var updatedProject = await projectRepository.Update(project, cancellationToken);

        return updatedProject is null ? null : ProjectMapper.MapToDTO(updatedProject);
    }

    public async Task<bool> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        return await projectRepository.Delete(id, cancellationToken);
    }
}
