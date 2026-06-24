using Microsoft.Extensions.Logging;

namespace PortfolioWeb.Application.Logging;

internal static partial class ProjectServiceLogMessages
{
    [LoggerMessage(EventId = 2001, Level = LogLevel.Warning, Message = "Project retrieval rejected because the provided project id is empty.")]
    public static partial void ProjectRetrievalRejectedBecauseIdIsEmpty(this ILogger logger);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Warning, Message = "Project not found during retrieval. ProjectId: {ProjectId}")]
    public static partial void ProjectNotFoundDuringRetrieval(this ILogger logger, Guid projectId);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Warning, Message = "Project creation rejected because the provided author id is empty.")]
    public static partial void ProjectCreationRejectedBecauseAuthorIdIsEmpty(this ILogger logger);

    [LoggerMessage(EventId = 2004, Level = LogLevel.Information, Message = "Creating project. AuthorId: {AuthorId}, Title: {Title}, Version: {Version}")]
    public static partial void CreatingProject(this ILogger logger, Guid authorId, string title, int version);

    [LoggerMessage(EventId = 2005, Level = LogLevel.Warning, Message = "Project creation rejected because the referenced author was not found. AuthorId: {AuthorId}")]
    public static partial void ProjectCreationRejectedBecauseReferencedAuthorWasNotFound(this ILogger logger, Guid authorId);

    [LoggerMessage(EventId = 2006, Level = LogLevel.Warning, Message = "Project creation rejected because the current user does not own the requested author. CurrentAuthorId: {CurrentAuthorId}, RequestedAuthorId: {RequestedAuthorId}")]
    public static partial void ProjectCreationRejectedBecauseResourceIsForbidden(this ILogger logger, Guid currentAuthorId, Guid requestedAuthorId);

    [LoggerMessage(EventId = 2007, Level = LogLevel.Information, Message = "Project created successfully. ProjectId: {ProjectId}, AuthorId: {AuthorId}")]
    public static partial void ProjectCreatedSuccessfully(this ILogger logger, Guid projectId, Guid authorId);

    [LoggerMessage(EventId = 2008, Level = LogLevel.Warning, Message = "Project update rejected because the provided project id is empty.")]
    public static partial void ProjectUpdateRejectedBecauseIdIsEmpty(this ILogger logger);

    [LoggerMessage(EventId = 2009, Level = LogLevel.Warning, Message = "Project update rejected because the current user does not own the requested project. CurrentAuthorId: {CurrentAuthorId}, ProjectAuthorId: {ProjectAuthorId}, ProjectId: {ProjectId}")]
    public static partial void ProjectUpdateRejectedBecauseResourceIsForbidden(this ILogger logger, Guid currentAuthorId, Guid projectAuthorId, Guid projectId);

    [LoggerMessage(EventId = 2010, Level = LogLevel.Information, Message = "Updating project. ProjectId: {ProjectId}, Title: {Title}, Version: {Version}")]
    public static partial void UpdatingProject(this ILogger logger, Guid projectId, string title, int version);

    [LoggerMessage(EventId = 2011, Level = LogLevel.Warning, Message = "Project not found during update. ProjectId: {ProjectId}")]
    public static partial void ProjectNotFoundDuringUpdate(this ILogger logger, Guid projectId);

    [LoggerMessage(EventId = 2012, Level = LogLevel.Warning, Message = "Project not found during update persistence. ProjectId: {ProjectId}")]
    public static partial void ProjectNotFoundDuringUpdatePersistence(this ILogger logger, Guid projectId);

    [LoggerMessage(EventId = 2013, Level = LogLevel.Information, Message = "Project updated successfully. ProjectId: {ProjectId}")]
    public static partial void ProjectUpdatedSuccessfully(this ILogger logger, Guid projectId);

    [LoggerMessage(EventId = 2014, Level = LogLevel.Warning, Message = "Project deletion rejected because the provided project id is empty.")]
    public static partial void ProjectDeletionRejectedBecauseIdIsEmpty(this ILogger logger);

    [LoggerMessage(EventId = 2015, Level = LogLevel.Warning, Message = "Project deletion rejected because the current user does not own the requested project. CurrentAuthorId: {CurrentAuthorId}, ProjectAuthorId: {ProjectAuthorId}, ProjectId: {ProjectId}")]
    public static partial void ProjectDeletionRejectedBecauseResourceIsForbidden(this ILogger logger, Guid currentAuthorId, Guid projectAuthorId, Guid projectId);

    [LoggerMessage(EventId = 2016, Level = LogLevel.Information, Message = "Deleting project. ProjectId: {ProjectId}")]
    public static partial void DeletingProject(this ILogger logger, Guid projectId);

    [LoggerMessage(EventId = 2017, Level = LogLevel.Warning, Message = "Project not found during deletion. ProjectId: {ProjectId}")]
    public static partial void ProjectNotFoundDuringDeletion(this ILogger logger, Guid projectId);

    [LoggerMessage(EventId = 2018, Level = LogLevel.Information, Message = "Project deleted successfully. ProjectId: {ProjectId}")]
    public static partial void ProjectDeletedSuccessfully(this ILogger logger, Guid projectId);
}
