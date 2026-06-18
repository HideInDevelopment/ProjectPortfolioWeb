using Microsoft.Extensions.Logging;
using PortfolioWeb.Application.Contract.DTOs;
using PortfolioWeb.Application.Contract.Exceptions.Author;
using PortfolioWeb.Application.Contract.Exceptions.Project;
using PortfolioWeb.Application.Contract.Services;
using PortfolioWeb.Application.Logging;
using PortfolioWeb.Application.Mappers;
using PortfolioWeb.Core.Contracts.Repositories;

namespace PortfolioWeb.Application.Services;

public class ProjectService(
    IProjectRepository projectRepository,
    IAuthorRepository authorRepository,
    ILogger<ProjectService> logger) : IProjectService
{
    public async Task<ProjectDTO> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            logger.ProjectRetrievalRejectedBecauseIdIsEmpty();
            throw new InvalidProjectIdException();
        }

        var project = await projectRepository.GetById(id, cancellationToken);

        if (project is not null)
        {
            return ProjectMapper.MapToDTO(project);
        }
        
        logger.ProjectNotFoundDuringRetrieval(id);
        throw new ProjectNotFoundException(id);

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
            logger.ProjectCreationRejectedBecauseAuthorIdIsEmpty();
            throw new InvalidAuthorIdException();
        }

        logger.CreatingProject(projectDto.AuthorId, projectDto.Title, projectDto.Version);

        var author = await authorRepository.GetById(projectDto.AuthorId, cancellationToken);

        if (author is null)
        {
            logger.ProjectCreationRejectedBecauseReferencedAuthorWasNotFound(projectDto.AuthorId);
            throw new ReferencedAuthorNotFoundException(projectDto.AuthorId);
        }

        var project = ProjectMapper.MapToEntity(projectDto);
        var createdProject = await projectRepository.Create(project, cancellationToken);

        logger.ProjectCreatedSuccessfully(createdProject.Id, createdProject.AuthorId);

        return ProjectMapper.MapToDTO(createdProject);
    }

    public async Task<ProjectDTO> Update(Guid id, UpdateProjectDTO projectDto, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            logger.ProjectUpdateRejectedBecauseIdIsEmpty();
            throw new InvalidProjectIdException();
        }

        logger.UpdatingProject(id, projectDto.Title, projectDto.Version);

        var project = await projectRepository.GetById(id, cancellationToken);

        if (project is null)
        {
            logger.ProjectNotFoundDuringUpdate(id);
            throw new ProjectNotFoundException(id);
        }

        project.Title = projectDto.Title;
        project.Description = projectDto.Description;
        project.Version = projectDto.Version;
        project.IsInDevelopment = projectDto.IsInDevelopment;

        var updatedProject = await projectRepository.Update(project, cancellationToken);

        if (updatedProject is null)
        {
            logger.ProjectNotFoundDuringUpdatePersistence(id);
            throw new ProjectNotFoundException(id);
        }

        logger.ProjectUpdatedSuccessfully(updatedProject.Id);

        return ProjectMapper.MapToDTO(updatedProject);
    }

    public async Task Delete(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            logger.ProjectDeletionRejectedBecauseIdIsEmpty();
            throw new InvalidProjectIdException();
        }

        logger.DeletingProject(id);

        var project = await projectRepository.GetById(id, cancellationToken);

        if (project is null)
        {
            logger.ProjectNotFoundDuringDeletion(id);
            throw new ProjectNotFoundException(id);
        }

        await projectRepository.Delete(project, cancellationToken);

        logger.ProjectDeletedSuccessfully(id);
    }
}
