using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PortfolioWeb.Api.ExceptionHandling;
using PortfolioWeb.Application.Contract.Exceptions.Auth;
using PortfolioWeb.Application.Contract.Exceptions.Author;
using PortfolioWeb.Application.Contract.Exceptions.Project;
using PortfolioWeb.Core.Contracts.Exceptions;

namespace PortfolioWeb.Api.Tests.ExceptionHandling;

public class GlobalExceptionHandlerTest
{
    private static IEnumerable<TestCaseData> ExceptionMappings()
    {
        yield return new TestCaseData(
            new InvalidAuthRequestException("invalid auth request"),
            StatusCodes.Status400BadRequest,
            "Invalid auth request",
            string.Empty,
            LogLevel.Warning);
        yield return new TestCaseData(
            new ForbiddenResourceAccessException(),
            StatusCodes.Status403Forbidden,
            "Forbidden resource access",
            string.Empty,
            LogLevel.Warning);
        yield return new TestCaseData(
            new InvalidAuthorIdException(),
            StatusCodes.Status400BadRequest,
            "Invalid author id",
            string.Empty,
            LogLevel.Warning);
        yield return new TestCaseData(
            new DuplicateEmailException("manuel@portfolio.local"),
            StatusCodes.Status409Conflict,
            "Duplicate email",
            string.Empty,
            LogLevel.Warning);
        yield return new TestCaseData(
            new InvalidCredentialsException(),
            StatusCodes.Status401Unauthorized,
            "Invalid credentials",
            string.Empty,
            LogLevel.Warning);
        yield return new TestCaseData(
            new InactiveUserException(),
            StatusCodes.Status403Forbidden,
            "Inactive user",
            string.Empty,
            LogLevel.Warning);
        yield return new TestCaseData(
            new InvalidProjectIdException(),
            StatusCodes.Status400BadRequest,
            "Invalid project id",
            string.Empty,
            LogLevel.Warning);
        yield return new TestCaseData(
            new ReferencedAuthorNotFoundException(Guid.NewGuid()),
            StatusCodes.Status400BadRequest,
            "Referenced author not found",
            string.Empty,
            LogLevel.Warning);
        yield return new TestCaseData(
            new AuthorNotFoundException(Guid.NewGuid()),
            StatusCodes.Status404NotFound,
            "Author not found",
            string.Empty,
            LogLevel.Warning);
        yield return new TestCaseData(
            new ProjectNotFoundException(Guid.NewGuid()),
            StatusCodes.Status404NotFound,
            "Project not found",
            string.Empty,
            LogLevel.Warning);
        yield return new TestCaseData(
            new InfrastructureConnectionException("connection failed"),
            StatusCodes.Status503ServiceUnavailable,
            "Database unavailable",
            "A required backend dependency is currently unavailable.",
            LogLevel.Error);
        yield return new TestCaseData(
            new InfrastructureQueryException("query failed"),
            StatusCodes.Status500InternalServerError,
            "Database query error",
            "The server could not complete the request.",
            LogLevel.Error);
        yield return new TestCaseData(
            new InfrastructurePersistenceException("persistence failed"),
            StatusCodes.Status500InternalServerError,
            "Database persistence error",
            "The server could not complete the request.",
            LogLevel.Error);
        yield return new TestCaseData(
            new Exception("unexpected failure"),
            StatusCodes.Status500InternalServerError,
            "Internal Server Error",
            "An unexpected error occurred.",
            LogLevel.Error);
    }

    [TestCaseSource(nameof(ExceptionMappings))]
    public async Task TryHandleAsync_ShouldWriteExpectedProblemDetailsAndLogLevel(
        Exception exception,
        int expectedStatusCode,
        string expectedTitle,
        string expectedDetail,
        LogLevel expectedLogLevel)
    {
        var logger = new TestLogger<GlobalExceptionHandler>();
        var handler = new GlobalExceptionHandler(logger);
        var httpContext = new DefaultHttpContext
        {
            TraceIdentifier = "trace-id",
            Request =
            {
                Method = HttpMethods.Get,
                Path = "/api/test"
            },
            Response =
            {
                Body = new MemoryStream()
            }
        };

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
            Assert.That(problemDetails.Detail, Is.EqualTo(expectedLogLevel == LogLevel.Error ? expectedDetail : exception.Message));
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
            Entries.Add(new LogEntry(logLevel, exception));
        }
    }

    private sealed record LogEntry(LogLevel LogLevel, Exception? Exception);

    private sealed class ProblemDetailsResponse
    {
        public int? Status { get; init; }

        public string Title { get; init; } = string.Empty;

        public string Detail { get; init; } = string.Empty;

        public string Instance { get; init; } = string.Empty;
    }
}
