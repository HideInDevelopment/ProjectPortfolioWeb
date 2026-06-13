using PortfolioWeb.Application.Contract.DTOs;
using PortfolioWeb.Application.Contract.Exceptions.Author;
using PortfolioWeb.Application.Contract.Exceptions.Project;
using PortfolioWeb.Application.Contract.Services;
using PortfolioWeb.Application.Mappers;
using PortfolioWeb.Domain.Contract.Repositories;

namespace PortfolioWeb.Application.Services;

public class ProjectService(
    IProjectRepository projectRepository,
    IAuthorRepository authorRepository) : IProjectService
{
    public async Task<ProjectDTO> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new InvalidProjectIdException();
        }

        var project = await projectRepository.GetById(id, cancellationToken);

        return project is null 
            ? throw new ProjectNotFoundException(id) 
            : ProjectMapper.MapToDTO(project);
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
        if (projectDto.AuthorId == Guid.Empty)
        {
            throw new InvalidAuthorIdException();
        }

        var author = await authorRepository.GetById(projectDto.AuthorId, cancellationToken);

        if (author is null)
        {
            throw new ReferencedAuthorNotFoundException(projectDto.AuthorId);
        }

        var project = ProjectMapper.MapToEntity(projectDto);
        var createdProject = await projectRepository.Create(project, cancellationToken);

        return ProjectMapper.MapToDTO(createdProject);
    }

    public async Task<ProjectDTO> Update(Guid id, UpdateProjectDTO projectDto, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new InvalidProjectIdException();
        }

        var project = await projectRepository.GetById(id, cancellationToken);

        if (project is null)
        {
            throw new ProjectNotFoundException(id);
        }

        project.Title = projectDto.Title;
        project.Description = projectDto.Description;
        project.Version = projectDto.Version;
        project.IsInDevelopment = projectDto.IsInDevelopment;

        var updatedProject = await projectRepository.Update(project, cancellationToken);

        return updatedProject is null 
            ? throw new ProjectNotFoundException(id) 
            : ProjectMapper.MapToDTO(updatedProject);
    }

    public async Task<bool> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new InvalidProjectIdException();
        }

        var project = await projectRepository.GetById(id, cancellationToken);

        if (project is null)
        {
            throw new ProjectNotFoundException(id);
        }

        var wasDeleted = await projectRepository.Delete(project, cancellationToken);

        return wasDeleted 
            ? true
            : throw new Exception("An unexpected error occurred while deleting the project.");
    }
}
