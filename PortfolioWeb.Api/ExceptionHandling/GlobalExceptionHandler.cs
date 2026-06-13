using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PortfolioWeb.Application.Contract.Exceptions.Author;
using PortfolioWeb.Application.Contract.Exceptions.Project;

namespace PortfolioWeb.Api.ExceptionHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = CreateProblemDetails(httpContext, exception);

        httpContext.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

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
            InvalidAuthorIdException => (StatusCodes.Status400BadRequest, "Invalid author id"),
            InvalidProjectIdException => (StatusCodes.Status400BadRequest, "Invalid project id"),
            ReferencedAuthorNotFoundException => (StatusCodes.Status400BadRequest, "Referenced author not found"),
            AuthorNotFoundException => (StatusCodes.Status404NotFound, "Author not found"),
            ProjectNotFoundException => (StatusCodes.Status404NotFound, "Project not found"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };
    }
}
