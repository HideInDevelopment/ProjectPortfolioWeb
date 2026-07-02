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
        var (statusCode, title, detail) = MapException(exception);

        return new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
    }

    private static (int StatusCode, string Title, string Detail) MapException(Exception exception)
    {
        return exception switch
        {
            InvalidAuthRequestException => (StatusCodes.Status400BadRequest, "Invalid auth request", exception.Message),
            ForbiddenResourceAccessException => (StatusCodes.Status403Forbidden, "Forbidden resource access", exception.Message),
            InvalidAuthorIdException => (StatusCodes.Status400BadRequest, "Invalid author id", exception.Message),
            DuplicateEmailException => (StatusCodes.Status409Conflict, "Duplicate email", exception.Message),
            InvalidCredentialsException => (StatusCodes.Status401Unauthorized, "Invalid credentials", exception.Message),
            InactiveUserException => (StatusCodes.Status403Forbidden, "Inactive user", exception.Message),
            InvalidProjectIdException => (StatusCodes.Status400BadRequest, "Invalid project id", exception.Message),
            ReferencedAuthorNotFoundException => (StatusCodes.Status400BadRequest, "Referenced author not found", exception.Message),
            AuthorNotFoundException => (StatusCodes.Status404NotFound, "Author not found", exception.Message),
            ProjectNotFoundException => (StatusCodes.Status404NotFound, "Project not found", exception.Message),
            InfrastructureConnectionException => (StatusCodes.Status503ServiceUnavailable, "Database unavailable", "A required backend dependency is currently unavailable."),
            InfrastructureQueryException => (StatusCodes.Status500InternalServerError, "Database query error", "The server could not complete the request."),
            InfrastructurePersistenceException => (StatusCodes.Status500InternalServerError, "Database persistence error", "The server could not complete the request."),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred.")
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
