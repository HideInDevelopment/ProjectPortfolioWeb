using Microsoft.Extensions.Logging;
using PortfolioWeb.Application.Contract.Dtos;
using PortfolioWeb.Application.Contract.Exceptions.Auth;
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
    public async Task<ProjectDto> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            logger.ProjectRetrievalRejectedBecauseIdIsEmpty();
            throw new InvalidProjectIdException();
        }

        var project = await projectRepository.GetById(id, cancellationToken);

        if (project is not null)
        {
            return ProjectMapper.MapToDto(project);
        }
        
        logger.ProjectNotFoundDuringRetrieval(id);
        throw new ProjectNotFoundException(id);

    }

    public async Task<List<ProjectDto>> GetAll(CancellationToken cancellationToken = default)
    {
        var projects = await projectRepository.GetAll(cancellationToken);

        return projects
            .Select(ProjectMapper.MapToDto)
            .ToList();
    }

    public async Task<ProjectDto> Create(CreateProjectDto projectDto, Guid currentAuthorId, CancellationToken cancellationToken = default)
    {
        if (currentAuthorId == Guid.Empty)
        {
            logger.ProjectCreationRejectedBecauseAuthorIdIsEmpty();
            throw new InvalidAuthorIdException();
        }

        logger.CreatingProject(currentAuthorId, projectDto.Title, projectDto.Version);

        var author = await authorRepository.GetById(currentAuthorId, cancellationToken);

        if (author is null)
        {
            logger.ProjectCreationRejectedBecauseReferencedAuthorWasNotFound(currentAuthorId);
            throw new ReferencedAuthorNotFoundException(currentAuthorId);
        }

        var project = ProjectMapper.MapToEntity(projectDto, currentAuthorId);
        var createdProject = await projectRepository.Create(project, cancellationToken);

        logger.ProjectCreatedSuccessfully(createdProject.Id, createdProject.AuthorId);

        return ProjectMapper.MapToDto(createdProject);
    }

    public async Task<ProjectDto> Update(Guid id, UpdateProjectDto projectDto, Guid currentAuthorId, CancellationToken cancellationToken = default)
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

        if (project.AuthorId != currentAuthorId)
        {
            logger.ProjectUpdateRejectedBecauseResourceIsForbidden(currentAuthorId, project.AuthorId, id);
            throw new ForbiddenResourceAccessException();
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

        return ProjectMapper.MapToDto(updatedProject);
    }

    public async Task Delete(Guid id, Guid currentAuthorId, CancellationToken cancellationToken = default)
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

        if (project.AuthorId != currentAuthorId)
        {
            logger.ProjectDeletionRejectedBecauseResourceIsForbidden(currentAuthorId, project.AuthorId, id);
            throw new ForbiddenResourceAccessException();
        }

        await projectRepository.Delete(project, cancellationToken);

        logger.ProjectDeletedSuccessfully(id);
    }
}

