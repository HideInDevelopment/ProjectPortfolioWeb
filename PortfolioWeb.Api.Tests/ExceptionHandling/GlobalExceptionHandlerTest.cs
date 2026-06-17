using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PortfolioWeb.Api.ExceptionHandling;
using PortfolioWeb.Application.Contract.Exceptions.Author;
using PortfolioWeb.Application.Contract.Exceptions.Project;
using PortfolioWeb.Core.Contracts.Exceptions;

namespace PortfolioWeb.Api.Tests.ExceptionHandling;

public class GlobalExceptionHandlerTest
{
    private static IEnumerable<TestCaseData> ExceptionMappings()
    {
        yield return new TestCaseData(
            new InvalidAuthorIdException(),
            StatusCodes.Status400BadRequest,
            "Invalid author id",
            LogLevel.Warning);
        yield return new TestCaseData(
            new InvalidProjectIdException(),
            StatusCodes.Status400BadRequest,
            "Invalid project id",
            LogLevel.Warning);
        yield return new TestCaseData(
            new ReferencedAuthorNotFoundException(Guid.NewGuid()),
            StatusCodes.Status400BadRequest,
            "Referenced author not found",
            LogLevel.Warning);
        yield return new TestCaseData(
            new AuthorNotFoundException(Guid.NewGuid()),
            StatusCodes.Status404NotFound,
            "Author not found",
            LogLevel.Warning);
        yield return new TestCaseData(
            new ProjectNotFoundException(Guid.NewGuid()),
            StatusCodes.Status404NotFound,
            "Project not found",
            LogLevel.Warning);
        yield return new TestCaseData(
            new InfrastructureConnectionException("connection failed"),
            StatusCodes.Status503ServiceUnavailable,
            "Database unavailable",
            LogLevel.Error);
        yield return new TestCaseData(
            new InfrastructureQueryException("query failed"),
            StatusCodes.Status500InternalServerError,
            "Database query error",
            LogLevel.Error);
        yield return new TestCaseData(
            new InfrastructurePersistenceException("persistence failed"),
            StatusCodes.Status500InternalServerError,
            "Database persistence error",
            LogLevel.Error);
        yield return new TestCaseData(
            new Exception("unexpected failure"),
            StatusCodes.Status500InternalServerError,
            "Internal Server Error",
            LogLevel.Error);
    }

    [TestCaseSource(nameof(ExceptionMappings))]
    public async Task TryHandleAsync_ShouldWriteExpectedProblemDetailsAndLogLevel(
        Exception exception,
        int expectedStatusCode,
        string expectedTitle,
        LogLevel expectedLogLevel)
    {
        var logger = new TestLogger<GlobalExceptionHandler>();
        var handler = new GlobalExceptionHandler(logger);
        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "trace-id";
        httpContext.Request.Method = HttpMethods.Get;
        httpContext.Request.Path = "/api/test";
        httpContext.Response.Body = new MemoryStream();

        var result = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        httpContext.Response.Body.Position = 0;
        var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetailsResponse>(
            httpContext.Response.Body,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            },
            cancellationToken: CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(httpContext.Response.StatusCode, Is.EqualTo(expectedStatusCode));
            Assert.That(httpContext.Response.ContentType, Does.StartWith("application/problem+json"));
            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails!.Status, Is.EqualTo(expectedStatusCode));
            Assert.That(problemDetails.Title, Is.EqualTo(expectedTitle));
            Assert.That(problemDetails.Detail, Is.EqualTo(exception.Message));
            Assert.That(problemDetails.Instance, Is.EqualTo("/api/test"));
            Assert.That(logger.Entries, Has.Count.EqualTo(1));
            Assert.That(logger.Entries[0].LogLevel, Is.EqualTo(expectedLogLevel));
            Assert.That(logger.Entries[0].Exception, Is.EqualTo(expectedLogLevel == LogLevel.Error ? exception : null));
        });
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public List<LogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add(new LogEntry(logLevel, formatter(state, exception), exception));
        }
    }

    private sealed record LogEntry(LogLevel LogLevel, string Message, Exception? Exception);

    private sealed class ProblemDetailsResponse
    {
        public int? Status { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Detail { get; set; } = string.Empty;

        public string Instance { get; set; } = string.Empty;
    }
}
