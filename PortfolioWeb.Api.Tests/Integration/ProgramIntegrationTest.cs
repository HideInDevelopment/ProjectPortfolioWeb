using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PortfolioWeb.Application.Contract.DTOs;
using PortfolioWeb.Application.Contract.Exceptions.Author;
using PortfolioWeb.Application.Contract.Exceptions.Project;
using PortfolioWeb.Application.Contract.Services;
using PortfolioWeb.Core.Contracts.Exceptions;

namespace PortfolioWeb.Api.Tests.Integration;

public class ProgramIntegrationTest
{
    private TestWebApplicationFactory factory = null!;

    [SetUp]
    public void SetUp()
    {
        factory = new TestWebApplicationFactory();
    }

    [TearDown]
    public void TearDown()
    {
        factory.Dispose();
    }

    [Test]
    public async Task OpenApiEndpoint_ShouldBeAvailable()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
        });
    }

    [Test]
    public async Task ScalarEndpoint_ShouldBeAvailable()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/scalar");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task ExceptionHandler_ShouldReturnProblemDetails_WhenControllerThrowsKnownException()
    {
        using var client = CreateClientWithAuthorService(new ThrowingAuthorService(
            new InvalidAuthorIdException()));

        var response = await client.GetAsync($"/api/Authors/{Guid.NewGuid()}");
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails!.Title, Is.EqualTo("Invalid author id"));
            Assert.That(problemDetails.Status, Is.EqualTo((int)HttpStatusCode.BadRequest));
        });
    }

    [TestCaseSource(nameof(AuthorExceptionMappings))]
    public async Task ExceptionHandler_ShouldReturnExpectedProblemDetails_ForAuthorExceptions(
        Exception exception,
        HttpStatusCode expectedStatusCode,
        string expectedTitle)
    {
        using var client = CreateClientWithAuthorService(new ThrowingAuthorService(exception));

        var response = await client.GetAsync($"/api/Authors/{Guid.NewGuid()}");
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(expectedStatusCode));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails!.Title, Is.EqualTo(expectedTitle));
            Assert.That(problemDetails.Status, Is.EqualTo((int)expectedStatusCode));
        });
    }

    [TestCaseSource(nameof(ProjectExceptionMappings))]
    public async Task ExceptionHandler_ShouldReturnExpectedProblemDetails_ForProjectExceptions(
        Exception exception,
        HttpStatusCode expectedStatusCode,
        string expectedTitle)
    {
        using var client = CreateClientWithProjectService(new ThrowingProjectService(exception));

        var response = await client.GetAsync($"/api/Projects/{Guid.NewGuid()}");
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(expectedStatusCode));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails!.Title, Is.EqualTo(expectedTitle));
            Assert.That(problemDetails.Status, Is.EqualTo((int)expectedStatusCode));
        });
    }

    [Test]
    public async Task ExceptionHandler_ShouldReturnInternalServerError_WhenControllerThrowsUnhandledException()
    {
        using var client = CreateClientWithAuthorService(new ThrowingAuthorService(
            new Exception("unexpected failure")));

        var response = await client.GetAsync($"/api/Authors/{Guid.NewGuid()}");
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails!.Title, Is.EqualTo("Internal Server Error"));
            Assert.That(problemDetails.Status, Is.EqualTo((int)HttpStatusCode.InternalServerError));
        });
    }

    [Test]
    public async Task CreateProject_ShouldReturnBadRequest_WhenPayloadFailsDtoValidation()
    {
        using var client = CreateClientWithProjectService(new ThrowingProjectService(
            new Exception("Project service should not be reached for invalid payloads.")));
        var payload = new
        {
            Title = new string('A', 201),
            Description = "Valid description",
            ReleaseDate = "2026-07-01T00:00:00+00:00",
            Version = 1,
            AuthorId = Guid.NewGuid(),
            IsInDevelopment = true
        };

        using var response = await client.PostAsync(
            "/api/Projects",
            CreateJsonContent(payload));

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
        });
    }

    [Test]
    public async Task UpdateProject_ShouldReturnBadRequest_WhenPayloadFailsDtoValidation()
    {
        using var client = CreateClientWithProjectService(new ThrowingProjectService(
            new Exception("Project service should not be reached for invalid payloads.")));
        var payload = new
        {
            Title = string.Empty,
            Description = "Valid description",
            Version = 1,
            IsInDevelopment = true
        };

        using var response = await client.PutAsync(
            $"/api/Projects/{Guid.NewGuid()}",
            CreateJsonContent(payload));

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
        });
    }

    private HttpClient CreateClientWithAuthorService(IAuthorService authorService)
    {
        return factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IAuthorService>();
                    services.AddSingleton(authorService);
                });
            })
            .CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = true
            });
    }

    private HttpClient CreateClientWithProjectService(IProjectService projectService)
    {
        return factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IProjectService>();
                    services.AddSingleton(projectService);
                });
            })
            .CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = true
            });
    }

    private static StringContent CreateJsonContent<T>(T payload)
    {
        return new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");
    }

    private sealed class ThrowingAuthorService(Exception exception) : IAuthorService
    {
        public Task<AuthorDTO> GetById(Guid id, CancellationToken cancellationToken = default) =>
            throw exception;

        public Task<List<AuthorDTO>> GetAll(CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<AuthorDTO> Create(PersistAuthorDTO authorDto, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<AuthorDTO> Update(Guid id, PersistAuthorDTO authorDto, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task Delete(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private sealed class ThrowingProjectService(Exception exception) : IProjectService
    {
        public Task<ProjectDTO> GetById(Guid id, CancellationToken cancellationToken = default) =>
            throw exception;

        public Task<List<ProjectDTO>> GetAll(CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<ProjectDTO> Create(CreateProjectDTO projectDto, CancellationToken cancellationToken = default) =>
            throw exception;

        public Task<ProjectDTO> Update(Guid id, UpdateProjectDTO projectDto, CancellationToken cancellationToken = default) =>
            throw exception;

        public Task Delete(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private sealed class ProblemDetailsResponse
    {
        public int? Status { get; set; }

        public string Title { get; set; } = string.Empty;
    }

    private static IEnumerable<TestCaseData> AuthorExceptionMappings()
    {
        yield return new TestCaseData(
            new InvalidAuthorIdException(),
            HttpStatusCode.BadRequest,
            "Invalid author id");
        yield return new TestCaseData(
            new AuthorNotFoundException(Guid.NewGuid()),
            HttpStatusCode.NotFound,
            "Author not found");
        yield return new TestCaseData(
            new InfrastructureConnectionException("db unavailable"),
            HttpStatusCode.ServiceUnavailable,
            "Database unavailable");
        yield return new TestCaseData(
            new InfrastructureQueryException("query error"),
            HttpStatusCode.InternalServerError,
            "Database query error");
        yield return new TestCaseData(
            new InfrastructurePersistenceException("persistence error"),
            HttpStatusCode.InternalServerError,
            "Database persistence error");
    }

    private static IEnumerable<TestCaseData> ProjectExceptionMappings()
    {
        yield return new TestCaseData(
            new InvalidProjectIdException(),
            HttpStatusCode.BadRequest,
            "Invalid project id");
        yield return new TestCaseData(
            new ProjectNotFoundException(Guid.NewGuid()),
            HttpStatusCode.NotFound,
            "Project not found");
        yield return new TestCaseData(
            new ReferencedAuthorNotFoundException(Guid.NewGuid()),
            HttpStatusCode.BadRequest,
            "Referenced author not found");
    }
}
