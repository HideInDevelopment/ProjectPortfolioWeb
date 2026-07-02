namespace PortfolioWeb.Api.ExceptionHandling;

internal static partial class GlobalExceptionHandlerLogMessages
{
    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Warning,
        Message = "Handled HTTP exception. StatusCode: {StatusCode}, Title: {Title}, ExceptionType: {ExceptionType}, Method: {HttpMethod}, Path: {Path}, TraceId: {TraceId}")]
    public static partial void HandledWarning(
        this ILogger logger,
        int statusCode,
        string title,
        string exceptionType,
        string httpMethod,
        string path,
        string traceId);

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Error,
        Message = "Handled HTTP exception. StatusCode: {StatusCode}, Title: {Title}, ExceptionType: {ExceptionType}, Method: {HttpMethod}, Path: {Path}, TraceId: {TraceId}")]
    public static partial void HandledError(
        this ILogger logger,
        Exception exception,
        int statusCode,
        string title,
        string exceptionType,
        string httpMethod,
        string path,
        string traceId);
}
