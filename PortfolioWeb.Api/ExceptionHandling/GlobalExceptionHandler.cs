using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PortfolioWeb.Application.Contract.Exceptions.Auth;
using PortfolioWeb.Application.Contract.Exceptions.Author;
using PortfolioWeb.Application.Contract.Exceptions.Project;
using PortfolioWeb.Core.Contracts.Exceptions;

namespace PortfolioWeb.Api.ExceptionHandling;

public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = CreateProblemDetails(httpContext, exception);
        var statusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;

        LogException(httpContext, exception, statusCode, problemDetails.Title ?? "Internal Server Error");

        httpContext.Response.StatusCode = statusCode;
        // TODO: Revisit whether ASP.NET offers a cleaner built-in ProblemDetails writer path that preserves
        // `application/problem+json` without having to force the content type explicitly here.
        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);

        return true;
    }

    private static ProblemDetails CreateProblemDetails(HttpContext httpContext, Exception exception)
    {
        var (statusCode, title) = MapException(exception);

        return new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };
    }

    private static (int StatusCode, string Title) MapException(Exception exception)
    {
        return exception switch
        {
            InvalidAuthRequestException => (StatusCodes.Status400BadRequest, "Invalid auth request"),
            InvalidAuthorIdException => (StatusCodes.Status400BadRequest, "Invalid author id"),
            AuthorCreationRequiresUserException => (StatusCodes.Status400BadRequest, "Author creation requires user registration"),
            DuplicateEmailException => (StatusCodes.Status409Conflict, "Duplicate email"),
            InvalidCredentialsException => (StatusCodes.Status401Unauthorized, "Invalid credentials"),
            InactiveUserException => (StatusCodes.Status403Forbidden, "Inactive user"),
            InvalidProjectIdException => (StatusCodes.Status400BadRequest, "Invalid project id"),
            ReferencedAuthorNotFoundException => (StatusCodes.Status400BadRequest, "Referenced author not found"),
            AuthorNotFoundException => (StatusCodes.Status404NotFound, "Author not found"),
            ProjectNotFoundException => (StatusCodes.Status404NotFound, "Project not found"),
            InfrastructureConnectionException => (StatusCodes.Status503ServiceUnavailable, "Database unavailable"),
            InfrastructureQueryException => (StatusCodes.Status500InternalServerError, "Database query error"),
            InfrastructurePersistenceException => (StatusCodes.Status500InternalServerError, "Database persistence error"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };
    }

    private void LogException(HttpContext httpContext, Exception exception, int statusCode, string title)
    {
        var exceptionType = exception.GetType().Name;
        var httpMethod = httpContext.Request.Method;
        var path = httpContext.Request.Path.Value ?? string.Empty;
        var traceId = httpContext.TraceIdentifier;

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.HandledError(
                exception,
                statusCode,
                title,
                exceptionType,
                httpMethod,
                path,
                traceId);

            return;
        }

        logger.HandledWarning(
            statusCode,
            title,
            exceptionType,
            httpMethod,
            path,
            traceId);
    }
}
