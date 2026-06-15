using Microsoft.Extensions.Logging;

namespace PortfolioWeb.Application.Logging;

internal static partial class AuthorServiceLogMessages
{
    [LoggerMessage(EventId = 1001, Level = LogLevel.Warning, Message = "Author retrieval rejected because the provided author id is empty.")]
    public static partial void AuthorRetrievalRejectedBecauseIdIsEmpty(this ILogger logger);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Warning, Message = "Author not found during retrieval. AuthorId: {AuthorId}")]
    public static partial void AuthorNotFoundDuringRetrieval(this ILogger logger, Guid authorId);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Information, Message = "Creating author. AuthorName: {AuthorName}")]
    public static partial void CreatingAuthor(this ILogger logger, string authorName);

    [LoggerMessage(EventId = 1004, Level = LogLevel.Information, Message = "Author created successfully. AuthorId: {AuthorId}, AuthorName: {AuthorName}")]
    public static partial void AuthorCreatedSuccessfully(this ILogger logger, Guid authorId, string authorName);

    [LoggerMessage(EventId = 1005, Level = LogLevel.Warning, Message = "Author update rejected because the provided author id is empty.")]
    public static partial void AuthorUpdateRejectedBecauseIdIsEmpty(this ILogger logger);

    [LoggerMessage(EventId = 1006, Level = LogLevel.Information, Message = "Updating author. AuthorId: {AuthorId}, AuthorName: {AuthorName}")]
    public static partial void UpdatingAuthor(this ILogger logger, Guid authorId, string authorName);

    [LoggerMessage(EventId = 1007, Level = LogLevel.Warning, Message = "Author not found during update. AuthorId: {AuthorId}")]
    public static partial void AuthorNotFoundDuringUpdate(this ILogger logger, Guid authorId);

    [LoggerMessage(EventId = 1008, Level = LogLevel.Information, Message = "Author updated successfully. AuthorId: {AuthorId}")]
    public static partial void AuthorUpdatedSuccessfully(this ILogger logger, Guid authorId);

    [LoggerMessage(EventId = 1009, Level = LogLevel.Warning, Message = "Author deletion rejected because the provided author id is empty.")]
    public static partial void AuthorDeletionRejectedBecauseIdIsEmpty(this ILogger logger);

    [LoggerMessage(EventId = 1010, Level = LogLevel.Information, Message = "Deleting author. AuthorId: {AuthorId}")]
    public static partial void DeletingAuthor(this ILogger logger, Guid authorId);

    [LoggerMessage(EventId = 1011, Level = LogLevel.Warning, Message = "Author not found during deletion. AuthorId: {AuthorId}")]
    public static partial void AuthorNotFoundDuringDeletion(this ILogger logger, Guid authorId);

    [LoggerMessage(EventId = 1012, Level = LogLevel.Information, Message = "Author deleted successfully. AuthorId: {AuthorId}")]
    public static partial void AuthorDeletedSuccessfully(this ILogger logger, Guid authorId);
}
