using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using PortfolioWeb.Application.Contract.Dtos;
using PortfolioWeb.Application.Contract.Exceptions.Auth;
using PortfolioWeb.Application.Contract.Exceptions.Author;
using PortfolioWeb.Application.Contract.Exceptions.Project;
using PortfolioWeb.Application.Contract.Services;
using PortfolioWeb.Core.Contracts.Exceptions;
using PortfolioWeb.Core.Contracts.Repositories;
using PortfolioWeb.Domain.Entities;
using PortfolioWeb.Infrastructure.Persistence;
using PortfolioWeb.Infrastructure.Repositories;

namespace PortfolioWeb.Api.Tests.Integration;

public class ProgramIntegrationTest
{
    private TestWebApplicationFactory _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new TestWebApplicationFactory();
    }

    [TearDown]
    public void TearDown()
    {
        _factory.Dispose();
    }

    [Test]
    public async Task OpenApiEndpoint_ShouldBeAvailable()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
        });
    }

    [Test]
    public async Task OpenApiEndpoint_ShouldBeUnavailableInProductionByDefault()
    {
        await EnsurePostgreSqlAvailableOrIgnoreAsync();

        using var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Production");
                builder.ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Features:ExposeApiDocs"] = "false"
                    });
                });
            })
            .CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task ScalarEndpoint_ShouldBeAvailable()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/scalar");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task OpenApiEndpoint_ShouldDescribeBearerSecurityForProtectedOperations()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/openapi/v1.json");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        var root = document.RootElement;
        var securitySchemes = root.GetProperty("components").GetProperty("securitySchemes");
        var projectsPost = root.GetProperty("paths").GetProperty("/api/Projects").GetProperty("post");

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(securitySchemes.TryGetProperty("Bearer", out _), Is.True);
            Assert.That(projectsPost.TryGetProperty("security", out var security), Is.True);
            Assert.That(security.GetArrayLength(), Is.GreaterThan(0));
        });
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
    public async Task Register_ShouldReturnJwtAcceptedByProtectedAuthorEndpoint()
    {
        var userRepository = new InMemoryUserRepository();
        using var client = CreateClientWithUserRepositoryAndAuthorService(
            userRepository,
            new OwnershipProbeAuthorService());

        using var registerResponse = await client.PostAsync(
            "/api/auth/register",
            CreateJsonContent(new
            {
                Email = "manuel@portfolio.local",
                Password = "password",
                AuthorName = "Manuel"
            }));
        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        Assert.That(registerResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(authResponse, Is.Not.Null);

        AuthenticateClient(client, authResponse!.AccessToken);

        using var updateResponse = await client.PutAsync(
            "/api/Authors",
            CreateJsonContent(new
            {
                Name = "Updated Manuel"
            }));
        var author = await updateResponse.Content.ReadFromJsonAsync<AuthorDto>();
        var createdUser = userRepository.CreatedUser;
        if (author is null)
        {
            Assert.Fail("Expected updated author payload.");
            return;
        }

        if (createdUser is null)
        {
            Assert.Fail("Expected created user to be stored by the in-memory repository.");
            return;
        }

        Assert.Multiple(() =>
        {
            Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(author.Id, Is.EqualTo(createdUser.Author.Id));
            Assert.That(author.Name, Is.EqualTo("Updated Manuel"));
        });
    }

    [Test]
    public async Task Login_ShouldUseHashCreatedDuringRegisterAndReturnJwtAcceptedByProtectedAuthorEndpoint()
    {
        var userRepository = new InMemoryUserRepository();
        using var client = CreateClientWithUserRepositoryAndAuthorService(
            userRepository,
            new OwnershipProbeAuthorService());

        using var registerResponse = await client.PostAsync(
            "/api/auth/register",
            CreateJsonContent(new
            {
                Email = "manuel@portfolio.local",
                Password = "password",
                AuthorName = "Manuel"
            }));

        Assert.That(registerResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var loginResponse = await client.PostAsync(
            "/api/auth/login",
            CreateJsonContent(new
            {
                Email = "manuel@portfolio.local",
                Password = "password"
            }));
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        Assert.That(loginResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(authResponse, Is.Not.Null);

        AuthenticateClient(client, authResponse!.AccessToken);

        using var updateResponse = await client.PutAsync(
            "/api/Authors",
            CreateJsonContent(new
            {
                Name = "Updated After Login"
            }));
        var author = await updateResponse.Content.ReadFromJsonAsync<AuthorDto>();
        var createdUser = userRepository.CreatedUser;
        if (author is null)
        {
            Assert.Fail("Expected updated author payload.");
            return;
        }

        if (createdUser is null)
        {
            Assert.Fail("Expected created user to be stored by the in-memory repository.");
            return;
        }

        Assert.Multiple(() =>
        {
            Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(author.Id, Is.EqualTo(createdUser.Author.Id));
            Assert.That(author.Name, Is.EqualTo("Updated After Login"));
        });
    }

    [Test]
    public async Task Register_Login_And_FullAuthenticatedFlow_ShouldPersistAndBeReadableThroughHttp()
    {
        using var client = _factory.CreateClient();
        var uniqueId = Guid.NewGuid().ToString("N");
        var email = $"manuel.{uniqueId}@portfolio.local";
        var authorName = $"Manuel {uniqueId}";

        await EnsurePostgreSqlAvailableOrIgnoreAsync();
        await ResetApiTestDatabaseAsync();

        using var registerResponse = await client.PostAsync(
            "/api/auth/register",
            CreateJsonContent(new
            {
                Email = email,
                Password = "password",
                AuthorName = authorName
            }));
        var registerAuthResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        using var loginResponse = await client.PostAsync(
            "/api/auth/login",
            CreateJsonContent(new
            {
                Email = email,
                Password = "password"
            }));
        var loginAuthResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        Assert.That(registerResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(registerAuthResponse, Is.Not.Null);
        Assert.That(loginResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(loginAuthResponse, Is.Not.Null);

        AuthenticateClient(client, loginAuthResponse!.AccessToken);

        using var updateAuthorResponse = await client.PutAsync(
            "/api/Authors",
            CreateJsonContent(new
            {
                Name = "Updated Manuel"
            }));
        var updatedAuthor = await updateAuthorResponse.Content.ReadFromJsonAsync<AuthorDto>();

        if (updatedAuthor is null)
        {
            Assert.Fail("Expected updated author payload.");
            return;
        }

        using var createProjectResponse = await client.PostAsync(
            "/api/Projects",
            CreateJsonContent(new
            {
                Title = "PortfolioWeb",
                Description = "Personal portfolio website.",
                ReleaseDate = "2026-07-01T00:00:00+00:00",
                Version = 1,
                IsInDevelopment = true
            }));
        var createdProject = await createProjectResponse.Content.ReadFromJsonAsync<ProjectDto>();

        if (createdProject is null)
        {
            Assert.Fail("Expected created project payload.");
            return;
        }

        using var updateProjectResponse = await client.PutAsync(
            $"/api/Projects/{createdProject.Id}",
            CreateJsonContent(new
            {
                Title = "PortfolioWeb API",
                Description = "Personal portfolio backend API.",
                Version = 2,
                IsInDevelopment = false
            }));
        var updatedProject = await updateProjectResponse.Content.ReadFromJsonAsync<ProjectDto>();

        if (updatedProject is null)
        {
            Assert.Fail("Expected updated project payload.");
            return;
        }

        using var getAuthorResponse = await client.GetAsync($"/api/Authors/{updatedAuthor.Id}");
        var reloadedAuthor = await getAuthorResponse.Content.ReadFromJsonAsync<AuthorDto>();

        if (reloadedAuthor is null)
        {
            Assert.Fail("Expected reloaded author payload.");
            return;
        }

        using var getProjectResponse = await client.GetAsync($"/api/Projects/{createdProject.Id}");
        var reloadedProject = await getProjectResponse.Content.ReadFromJsonAsync<ProjectDto>();

        if (reloadedProject is null)
        {
            Assert.Fail("Expected reloaded project payload.");
            return;
        }

        Assert.Multiple(() =>
        {
            Assert.That(registerResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
            Assert.That(updateAuthorResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(updatedAuthor.Name, Is.EqualTo("Updated Manuel"));
            Assert.That(createProjectResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(createdProject.AuthorId, Is.EqualTo(updatedAuthor.Id));
            Assert.That(createdProject.Title, Is.EqualTo("PortfolioWeb"));
            Assert.That(updateProjectResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(updatedProject.Title, Is.EqualTo("PortfolioWeb API"));
            Assert.That(updatedProject.Description, Is.EqualTo("Personal portfolio backend API."));
            Assert.That(updatedProject.Version, Is.EqualTo(2));
            Assert.That(updatedProject.IsInDevelopment, Is.False);
            Assert.That(getAuthorResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(reloadedAuthor.Id, Is.EqualTo(updatedAuthor.Id));
            Assert.That(reloadedAuthor.Name, Is.EqualTo("Updated Manuel"));
            Assert.That(reloadedAuthor.Projects, Has.Count.EqualTo(1));
            Assert.That(reloadedAuthor.Projects[0].Id, Is.EqualTo(createdProject.Id));
            Assert.That(reloadedAuthor.Projects[0].Title, Is.EqualTo("PortfolioWeb API"));
            Assert.That(getProjectResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(reloadedProject.Id, Is.EqualTo(createdProject.Id));
            Assert.That(reloadedProject.AuthorId, Is.EqualTo(updatedAuthor.Id));
            Assert.That(reloadedProject.Title, Is.EqualTo("PortfolioWeb API"));
            Assert.That(reloadedProject.Description, Is.EqualTo("Personal portfolio backend API."));
            Assert.That(reloadedProject.Version, Is.EqualTo(2));
            Assert.That(reloadedProject.IsInDevelopment, Is.False);
        });

        using var verificationScope = _factory.Services.CreateScope();
        var verificationDbContext = verificationScope.ServiceProvider.GetRequiredService<PortfolioWebDbContext>();
        var persistedAuthor = await verificationDbContext.Authors
            .Include(author => author.Projects)
            .SingleAsync(author => author.Id == updatedAuthor.Id);

        Assert.Multiple(() =>
        {
            Assert.That(persistedAuthor.Name, Is.EqualTo("Updated Manuel"));
            Assert.That(persistedAuthor.Projects, Has.Count.EqualTo(1));
            Assert.That(persistedAuthor.Projects[0].Id, Is.EqualTo(createdProject.Id));
            Assert.That(persistedAuthor.Projects[0].Title, Is.EqualTo("PortfolioWeb API"));
            Assert.That(persistedAuthor.Projects[0].Description, Is.EqualTo("Personal portfolio backend API."));
            Assert.That(persistedAuthor.Projects[0].Version, Is.EqualTo(2));
            Assert.That(persistedAuthor.Projects[0].IsInDevelopment, Is.False);
        });
    }

    [Test]
    public async Task Register_ShouldReturnConflict_WhenEmailAlreadyExists_WithRealServicesAndRepositories()
    {
        using var client = _factory.CreateClient();
        var uniqueId = Guid.NewGuid().ToString("N");
        var email = $"duplicate.{uniqueId}@portfolio.local";

        await EnsurePostgreSqlAvailableOrIgnoreAsync();
        await ResetApiTestDatabaseAsync();

        using var firstResponse = await client.PostAsync(
            "/api/auth/register",
            CreateJsonContent(new
            {
                Email = email,
                Password = "password",
                AuthorName = "First Author"
            }));

        using var secondResponse = await client.PostAsync(
            "/api/auth/register",
            CreateJsonContent(new
            {
                Email = email,
                Password = "password",
                AuthorName = "Second Author"
            }));
        var problemDetails = await secondResponse.Content.ReadFromJsonAsync<ProblemDetailsResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(firstResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(secondResponse.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails!.Title, Is.EqualTo("Duplicate email"));
        });
    }

    [Test]
    public async Task Register_ShouldReturnConflict_WhenCreateFailsBecauseEmailWasInsertedConcurrently()
    {
        using var client = CreateClientWithUserRepository(
            new RaceDuplicateUserRepository("manuel@portfolio.local", "Manuel"));

        using var response = await client.PostAsync(
            "/api/auth/register",
            CreateJsonContent(new
            {
                Email = "manuel@portfolio.local",
                Password = "password",
                AuthorName = "Manuel"
            }));
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails!.Title, Is.EqualTo("Duplicate email"));
            Assert.That(problemDetails.Status, Is.EqualTo((int)HttpStatusCode.Conflict));
        });
    }

    [Test]
    public async Task Register_ShouldReturnConflict_WhenRealPostgreSqlUniqueConstraintIsHitDuringRace()
    {
        var email = $"race.{Guid.NewGuid():N}@portfolio.local";
        using var client = CreateClientWithForcedDuplicateEmailRace(email);

        await EnsurePostgreSqlAvailableOrIgnoreAsync();
        await ResetApiTestDatabaseAsync();

        var firstRequest = client.PostAsync(
            "/api/auth/register",
            CreateJsonContent(new
            {
                Email = email,
                Password = "password",
                AuthorName = "First"
            }));

        var secondRequest = client.PostAsync(
            "/api/auth/register",
            CreateJsonContent(new
            {
                Email = email,
                Password = "password",
                AuthorName = "Second"
            }));

        await Task.WhenAll(firstRequest, secondRequest);

        var responses = new[] { firstRequest.Result, secondRequest.Result };
        var statusCodes = responses.Select(response => response.StatusCode).OrderBy(code => code).ToArray();
        var conflictResponse = responses.Single(response => response.StatusCode == HttpStatusCode.Conflict);
        var problemDetails = await conflictResponse.Content.ReadFromJsonAsync<ProblemDetailsResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(
                statusCodes,
                Is.EquivalentTo(new[] { HttpStatusCode.Conflict, HttpStatusCode.OK }),
                $"Actual statuses: {string.Join(", ", statusCodes)}");
            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails!.Title, Is.EqualTo("Duplicate email"));
        });
    }

    [Test]
    public async Task DeleteAuthor_ShouldReturnForbidden_WhenAuthenticatedUserTargetsAnotherAuthor_WithRealServicesAndRepositories()
    {
        using var client = _factory.CreateClient();
        var firstUser = CreatePersistedUser(
            $"owner.{Guid.NewGuid():N}@portfolio.local",
            "Owner Author",
            passwordHash: HashForTests("password"));
        var secondUser = CreatePersistedUser(
            $"other.{Guid.NewGuid():N}@portfolio.local",
            "Other Author",
            passwordHash: HashForTests("password"));

        await EnsurePostgreSqlAvailableOrIgnoreAsync();
        await ResetApiTestDatabaseAsync();

        using (var seedScope = _factory.Services.CreateScope())
        {
            var dbContext = seedScope.ServiceProvider.GetRequiredService<PortfolioWebDbContext>();
            await dbContext.Database.MigrateAsync();
            dbContext.Users.Add(firstUser);
            dbContext.Users.Add(secondUser);
            await dbContext.SaveChangesAsync();
        }

        using var loginResponse = await client.PostAsync(
            "/api/auth/login",
            CreateJsonContent(new
            {
                Email = firstUser.Email,
                Password = "password"
            }));
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        Assert.That(loginResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(authResponse, Is.Not.Null);

        AuthenticateClient(client, authResponse!.AccessToken);

        using var deleteResponse = await client.DeleteAsync($"/api/Authors/{secondUser.Author.Id}");
        var problemDetails = await deleteResponse.Content.ReadFromJsonAsync<ProblemDetailsResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails!.Title, Is.EqualTo("Forbidden resource access"));
        });
    }

    [Test]
    public async Task UpdateProject_ShouldReturnForbidden_WhenAuthenticatedUserTargetsAnotherAuthorsProject_WithRealServicesAndRepositories()
    {
        using var client = _factory.CreateClient();
        var firstUser = CreatePersistedUser(
            $"project.owner.{Guid.NewGuid():N}@portfolio.local",
            "Project Owner",
            passwordHash: HashForTests("password"));
        var secondUser = CreatePersistedUser(
            $"project.other.{Guid.NewGuid():N}@portfolio.local",
            "Project Other",
            passwordHash: HashForTests("password"));
        var secondUserProject = new Project(
            "Original Title",
            "Original Description",
            new DateTimeOffset(2026, 07, 02, 0, 0, 0, TimeSpan.Zero),
            1,
            secondUser.Author.Id,
            true)
        {
            Id = Guid.NewGuid()
        };

        await EnsurePostgreSqlAvailableOrIgnoreAsync();
        await ResetApiTestDatabaseAsync();

        using (var seedScope = _factory.Services.CreateScope())
        {
            var dbContext = seedScope.ServiceProvider.GetRequiredService<PortfolioWebDbContext>();
            await dbContext.Database.MigrateAsync();
            dbContext.Users.Add(firstUser);
            dbContext.Users.Add(secondUser);
            dbContext.Projects.Add(secondUserProject);
            await dbContext.SaveChangesAsync();
        }

        using var loginResponse = await client.PostAsync(
            "/api/auth/login",
            CreateJsonContent(new
            {
                Email = firstUser.Email,
                Password = "password"
            }));
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        Assert.That(loginResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(authResponse, Is.Not.Null);

        AuthenticateClient(client, authResponse!.AccessToken);

        using var updateResponse = await client.PutAsync(
            $"/api/Projects/{secondUserProject.Id}",
            CreateJsonContent(new
            {
                Title = "Updated",
                Description = "Updated description",
                Version = 2,
                IsInDevelopment = false
            }));
        var problemDetails = await updateResponse.Content.ReadFromJsonAsync<ProblemDetailsResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails!.Title, Is.EqualTo("Forbidden resource access"));
        });
    }

    [Test]
    public async Task DeleteEndpoints_ShouldReturnNoContent_WhenAuthenticatedOwner()
    {
        using var client = _factory.CreateClient();
        var uniqueId = Guid.NewGuid().ToString("N");
        var email = $"delete.{uniqueId}@portfolio.local";
        var authorName = $"Delete {uniqueId}";

        await EnsurePostgreSqlAvailableOrIgnoreAsync();
        await ResetApiTestDatabaseAsync();

        var seededUser = CreatePersistedUser(
            email,
            authorName,
            isActive: true,
            passwordHash: HashForTests("password"));

        using (var seedScope = _factory.Services.CreateScope())
        {
            var dbContext = seedScope.ServiceProvider.GetRequiredService<PortfolioWebDbContext>();
            await dbContext.Database.MigrateAsync();
            dbContext.Users.Add(seededUser);
            await dbContext.SaveChangesAsync();
        }

        using var loginResponse = await client.PostAsync(
            "/api/auth/login",
            CreateJsonContent(new
            {
                Email = email,
                Password = "password"
            }));
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        Assert.That(loginResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(authResponse, Is.Not.Null);

        AuthenticateClient(client, authResponse!.AccessToken);

        using var createResponse = await client.PostAsync(
            "/api/Projects",
            CreateJsonContent(new
            {
                Title = "PortfolioWeb",
                Description = "Personal portfolio website.",
                ReleaseDate = "2026-07-01T00:00:00+00:00",
                Version = 1,
                IsInDevelopment = true
            }));
        var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();

        using var deleteProjectResponse = await client.DeleteAsync($"/api/Projects/{createdProject!.Id}");
        using var deleteAuthorResponse = await client.DeleteAsync($"/api/Authors/{seededUser.Author.Id}");

        Assert.Multiple(() =>
        {
            Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(deleteProjectResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(deleteAuthorResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        });
    }

    [Test]
    public async Task ProtectedAuthorEndpoint_ShouldReturnUnauthorized_WhenJwtDoesNotContainAuthorId()
    {
        var userRepository = new InMemoryUserRepository();
        userRepository.AddUser(CreatePersistedUser("manuel@portfolio.local", "Manuel"));

        using var client = CreateClientWithUserRepositoryAndAuthorService(
            userRepository,
            new OwnershipProbeAuthorService());
        AuthenticateClient(client, CreateAccessTokenWithoutAuthorId("manuel@portfolio.local"));

        using var response = await client.PutAsync(
            "/api/Authors",
            CreateJsonContent(new
            {
                Name = "Updated Manuel"
            }));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ProtectedAuthorEndpoint_ShouldReturnForbidden_WhenUserIsDeactivatedAfterTokenIssuance()
    {
        var userRepository = new InMemoryUserRepository();
        using var client = CreateClientWithUserRepositoryAndAuthorService(
            userRepository,
            new OwnershipProbeAuthorService());

        using var registerResponse = await client.PostAsync(
            "/api/auth/register",
            CreateJsonContent(new
            {
                Email = "manuel@portfolio.local",
                Password = "password",
                AuthorName = "Manuel"
            }));
        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        Assert.That(registerResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(authResponse, Is.Not.Null);

        userRepository.CreatedUser!.IsActive = false;
        AuthenticateClient(client, authResponse!.AccessToken);

        using var response = await client.PutAsync(
            "/api/Authors",
            CreateJsonContent(new
            {
                Name = "Updated Manuel"
            }));
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails!.Title, Is.EqualTo("Inactive user"));
            Assert.That(problemDetails.Status, Is.EqualTo((int)HttpStatusCode.Forbidden));
        });
    }

    [Test]
    public async Task ProtectedAuthorEndpoint_ShouldReturnUnauthorized_WhenJwtIsExpired()
    {
        using var client = _factory.CreateClient();
        AuthenticateClient(client, CreateAccessToken(
            authorId: Guid.NewGuid(),
            expiresUtc: DateTime.UtcNow.AddMinutes(-5)));

        using var response = await client.PutAsync(
            "/api/Authors",
            CreateJsonContent(new
            {
                Name = "Updated Manuel"
            }));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ProtectedAuthorEndpoint_ShouldReturnUnauthorized_WhenJwtSignatureIsInvalid()
    {
        using var client = _factory.CreateClient();
        AuthenticateClient(client, CreateAccessToken(
            authorId: Guid.NewGuid(),
            signingKey: "ThisIsAnotherTestSigningKeyWithEnoughLength456!"));

        using var response = await client.PutAsync(
            "/api/Authors",
            CreateJsonContent(new
            {
                Name = "Updated Manuel"
            }));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ProtectedAuthorEndpoint_ShouldReturnUnauthorized_WhenJwtAudienceIsInvalid()
    {
        using var client = _factory.CreateClient();
        AuthenticateClient(client, CreateAccessToken(
            authorId: Guid.NewGuid(),
            audience: "AnotherAudience"));

        using var response = await client.PutAsync(
            "/api/Authors",
            CreateJsonContent(new
            {
                Name = "Updated Manuel"
            }));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ProtectedAuthorEndpoint_ShouldReturnUnauthorized_WhenJwtIssuerIsInvalid()
    {
        using var client = _factory.CreateClient();
        AuthenticateClient(client, CreateAccessToken(
            authorId: Guid.NewGuid(),
            issuer: "AnotherIssuer"));

        using var response = await client.PutAsync(
            "/api/Authors",
            CreateJsonContent(new
            {
                Name = "Updated Manuel"
            }));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task CreateProject_ShouldReturnBadRequest_WhenPayloadFailsDtoValidation()
    {
        using var client = CreateClientWithProjectService(new ThrowingProjectService(
            new Exception("Project service should not be reached for invalid payloads.")));
        var authorId = Guid.NewGuid();
        AuthenticateClient(client, authorId);
        var payload = new
        {
            Title = new string('A', 201),
            Description = "Valid description",
            ReleaseDate = "2026-07-01T00:00:00+00:00",
            Version = 1,
            IsInDevelopment = true
        };

        using var response = await client.PostAsync(
            "/api/Projects",
            CreateJsonContent(payload));
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails!.Title, Is.EqualTo("Validation failed"));
            Assert.That(problemDetails.Status, Is.EqualTo((int)HttpStatusCode.BadRequest));
            Assert.That(problemDetails.Detail, Is.EqualTo("One or more validation errors occurred."));
            Assert.That(problemDetails.Instance, Is.EqualTo("/api/Projects"));
            Assert.That(problemDetails!.Errors, Contains.Key("Title"));
            Assert.That(problemDetails.Errors["Title"], Has.Some.Contains("between 1 and 200"));
        });
    }

    [Test]
    public async Task CreateProject_ShouldReturnBadRequest_WhenDescriptionFailsDtoValidation()
    {
        using var client = CreateClientWithProjectService(new ThrowingProjectService(
            new Exception("Project service should not be reached for invalid payloads.")));
        AuthenticateClient(client, Guid.NewGuid());
        var payload = new
        {
            Title = "Valid title",
            Description = string.Empty,
            ReleaseDate = "2026-07-01T00:00:00+00:00",
            Version = 1,
            IsInDevelopment = true
        };

        using var response = await client.PostAsync(
            "/api/Projects",
            CreateJsonContent(payload));
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails!.Title, Is.EqualTo("Validation failed"));
            Assert.That(problemDetails!.Errors, Contains.Key("Description"));
            Assert.That(problemDetails.Errors["Description"], Is.Not.Empty);
        });
    }

    [Test]
    public async Task CreateProject_ShouldReturnBadRequest_WhenReleaseDateIsMalformed()
    {
        using var client = CreateClientWithProjectService(new ThrowingProjectService(
            new Exception("Project service should not be reached for invalid payloads.")));
        AuthenticateClient(client, Guid.NewGuid());
        var payload = new
        {
            Title = "Valid title",
            Description = "Valid description",
            ReleaseDate = "not-a-date",
            Version = 1,
            IsInDevelopment = true
        };

        using var response = await client.PostAsync(
            "/api/Projects",
            CreateJsonContent(payload));
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails!.Title, Is.EqualTo("Validation failed"));
            Assert.That(problemDetails!.Errors, Contains.Key("$.ReleaseDate"));
            Assert.That(problemDetails.Errors["$.ReleaseDate"], Has.Some.Contains("not a valid date"));
        });
    }

    [Test]
    public async Task CreateProject_ShouldReturnBadRequest_WhenReleaseDateIsMissing()
    {
        using var client = CreateClientWithProjectService(new ThrowingProjectService(
            new Exception("Project service should not be reached for invalid payloads.")));
        AuthenticateClient(client, Guid.NewGuid());
        var payload = new
        {
            Title = "Valid title",
            Description = "Valid description",
            Version = 1,
            IsInDevelopment = true
        };

        using var response = await client.PostAsync(
            "/api/Projects",
            CreateJsonContent(payload));
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails!.Errors, Contains.Key("ReleaseDate"));
            Assert.That(problemDetails.Errors["ReleaseDate"], Has.Some.Contains("Release date is required."));
        });
    }

    [Test]
    public async Task CreateProject_ShouldReturnBadRequest_WhenVersionIsNegative()
    {
        using var client = CreateClientWithProjectService(new ThrowingProjectService(
            new Exception("Project service should not be reached for invalid payloads.")));
        AuthenticateClient(client, Guid.NewGuid());
        var payload = new
        {
            Title = "Valid title",
            Description = "Valid description",
            ReleaseDate = "2026-07-01T00:00:00+00:00",
            Version = -1,
            IsInDevelopment = true
        };

        using var response = await client.PostAsync(
            "/api/Projects",
            CreateJsonContent(payload));
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails!.Errors, Contains.Key("Version"));
            Assert.That(problemDetails.Errors["Version"], Has.Some.Contains("zero or greater"));
        });
    }

    [Test]
    public async Task UpdateProject_ShouldReturnBadRequest_WhenPayloadFailsDtoValidation()
    {
        using var client = CreateClientWithProjectService(new ThrowingProjectService(
            new Exception("Project service should not be reached for invalid payloads.")));
        AuthenticateClient(client, Guid.NewGuid());
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
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails!.Title, Is.EqualTo("Validation failed"));
            Assert.That(problemDetails!.Errors, Contains.Key("Title"));
            Assert.That(problemDetails.Errors["Title"], Is.Not.Empty);
        });
    }

    [Test]
    public async Task UpdateProject_ShouldReturnBadRequest_WhenVersionIsNegative()
    {
        using var client = CreateClientWithProjectService(new ThrowingProjectService(
            new Exception("Project service should not be reached for invalid payloads.")));
        AuthenticateClient(client, Guid.NewGuid());
        var payload = new
        {
            Title = "Valid title",
            Description = "Valid description",
            Version = -1,
            IsInDevelopment = true
        };

        using var response = await client.PutAsync(
            $"/api/Projects/{Guid.NewGuid()}",
            CreateJsonContent(payload));
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails!.Errors, Contains.Key("Version"));
            Assert.That(problemDetails.Errors["Version"], Has.Some.Contains("zero or greater"));
        });
    }

    [Test]
    public async Task CreateProject_ShouldReturnUnauthorized_WhenRequestIsAnonymous()
    {
        using var client = _factory.CreateClient();
        var payload = new
        {
            Title = "PortfolioWeb",
            Description = "Personal portfolio website.",
            ReleaseDate = "2026-07-01T00:00:00+00:00",
            Version = 1,
            IsInDevelopment = true
        };

        using var response = await client.PostAsync(
            "/api/Projects",
            CreateJsonContent(payload));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task UpdateProject_ShouldReturnUnauthorized_WhenRequestIsAnonymous()
    {
        using var client = _factory.CreateClient();

        using var response = await client.PutAsync(
            $"/api/Projects/{Guid.NewGuid()}",
            CreateJsonContent(new
            {
                Title = "Updated",
                Description = "Updated description",
                Version = 1,
                IsInDevelopment = true
            }));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task DeleteProject_ShouldReturnUnauthorized_WhenRequestIsAnonymous()
    {
        using var client = _factory.CreateClient();

        using var response = await client.DeleteAsync($"/api/Projects/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task UpdateAuthor_ShouldReturnUnauthorized_WhenRequestIsAnonymous()
    {
        using var client = _factory.CreateClient();

        using var response = await client.PutAsync(
            "/api/Authors",
            CreateJsonContent(new
            {
                Name = "Updated"
            }));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task DeleteAuthor_ShouldReturnUnauthorized_WhenRequestIsAnonymous()
    {
        using var client = _factory.CreateClient();

        using var response = await client.DeleteAsync($"/api/Authors/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [TestCaseSource(nameof(AuthExceptionMappings))]
    public async Task ExceptionHandler_ShouldReturnExpectedProblemDetails_ForAuthExceptions(
        Exception exception,
        HttpStatusCode expectedStatusCode,
        string expectedTitle)
    {
        using var client = CreateClientWithAuthService(new ThrowingAuthService(exception));

        var response = await client.PostAsync(
            "/api/auth/login",
            CreateJsonContent(new
            {
                Email = "manuel@portfolio.local",
                Password = "password"
            }));
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
    public async Task Register_ShouldReturnBadRequest_WhenPayloadFailsDtoValidation()
    {
        using var client = CreateClientWithAuthService(new ThrowingAuthService(
            new Exception("Auth service should not be reached for invalid payloads.")));

        using var response = await client.PostAsync(
            "/api/auth/register",
            CreateJsonContent(new
            {
                Email = "",
                Password = "password",
                AuthorName = "Manuel"
            }));
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails!.Title, Is.EqualTo("Validation failed"));
            Assert.That(problemDetails.Instance, Is.EqualTo("/api/auth/register"));
            Assert.That(problemDetails!.Errors, Contains.Key("Email"));
            Assert.That(problemDetails.Errors["Email"], Is.Not.Empty);
        });
    }

    [Test]
    public async Task Register_ShouldReturnBadRequest_WhenEmailFormatIsInvalid()
    {
        using var client = CreateClientWithAuthService(new ThrowingAuthService(
            new Exception("Auth service should not be reached for invalid payloads.")));

        using var response = await client.PostAsync(
            "/api/auth/register",
            CreateJsonContent(new
            {
                Email = "not-an-email",
                Password = "password",
                AuthorName = "Manuel"
            }));
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails!.Errors, Contains.Key("Email"));
            Assert.That(problemDetails.Errors["Email"], Has.Some.Contains("valid email address"));
        });
    }

    [Test]
    public async Task Register_ShouldReturnBadRequest_WhenAuthorNameExceedsMaxLength()
    {
        using var client = CreateClientWithAuthService(new ThrowingAuthService(
            new Exception("Auth service should not be reached for invalid payloads.")));

        using var response = await client.PostAsync(
            "/api/auth/register",
            CreateJsonContent(new
            {
                Email = "manuel@portfolio.local",
                Password = "password",
                AuthorName = new string('A', 201)
            }));
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails!.Errors, Contains.Key("AuthorName"));
            Assert.That(problemDetails.Errors["AuthorName"], Has.Some.Contains("between 1 and 200"));
        });
    }

    [Test]
    public async Task UpdateAuthor_ShouldReturnBadRequest_WhenPayloadFailsDtoValidation()
    {
        using var client = CreateClientWithAuthorService(new ThrowingAuthorService(
            new Exception("Author service should not be reached for invalid payloads.")));
        AuthenticateClient(client, Guid.NewGuid());

        using var response = await client.PutAsync(
            "/api/Authors",
            CreateJsonContent(new
            {
                Name = string.Empty
            }));
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails!.Title, Is.EqualTo("Validation failed"));
            Assert.That(problemDetails!.Errors, Contains.Key("Name"));
            Assert.That(problemDetails.Errors["Name"], Is.Not.Empty);
        });
    }

    [Test]
    public async Task Login_ShouldReturnBadRequest_WhenPayloadFailsDtoValidation()
    {
        using var client = CreateClientWithAuthService(new ThrowingAuthService(
            new Exception("Auth service should not be reached for invalid payloads.")));

        using var response = await client.PostAsync(
            "/api/auth/login",
            CreateJsonContent(new
            {
                Email = "manuel@portfolio.local",
                Password = ""
            }));
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails!.Title, Is.EqualTo("Validation failed"));
            Assert.That(problemDetails!.Errors, Contains.Key("Password"));
            Assert.That(problemDetails.Errors["Password"], Is.Not.Empty);
        });
    }

    [Test]
    public async Task Login_ShouldReturnBadRequest_WhenEmailFormatIsInvalid()
    {
        using var client = CreateClientWithAuthService(new ThrowingAuthService(
            new Exception("Auth service should not be reached for invalid payloads.")));

        using var response = await client.PostAsync(
            "/api/auth/login",
            CreateJsonContent(new
            {
                Email = "not-an-email",
                Password = "password"
            }));
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsResponse>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails!.Errors, Contains.Key("Email"));
            Assert.That(problemDetails.Errors["Email"], Has.Some.Contains("valid email address"));
        });
    }

    private HttpClient CreateClientWithAuthorService(IAuthorService authorService)
    {
        return _factory.WithWebHostBuilder(builder =>
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
        return _factory.WithWebHostBuilder(builder =>
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

    private HttpClient CreateClientWithAuthService(IAuthService authService)
    {
        return _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IAuthService>();
                    services.AddSingleton(authService);
                });
            })
            .CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = true
            });
    }

    private HttpClient CreateClientWithUserRepository(IUserRepository userRepository)
    {
        return _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IUserRepository>();
                    services.AddSingleton(userRepository);
                });
            })
            .CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = true
            });
    }

    private HttpClient CreateClientWithForcedDuplicateEmailRace(string email)
    {
        return _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IUserRepository>();
                    services.AddSingleton(new DuplicateEmailRaceCoordinator(email));
                    services.AddScoped<UserRepository>();
                    services.AddScoped<IUserRepository, RealRaceDuplicateUserRepository>();
                });
            })
            .CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = true
            });
    }

    private HttpClient CreateClientWithUserRepositoryAndAuthorService(IUserRepository userRepository, IAuthorService authorService)
    {
        return _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IUserRepository>();
                    services.AddSingleton(userRepository);
                    services.RemoveAll<IAuthorService>();
                    services.AddSingleton(authorService);
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

    private static void AuthenticateClient(HttpClient client, Guid authorId)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CreateAccessToken(authorId));
    }

    private static void AuthenticateClient(HttpClient client, string accessToken)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    private static string CreateAccessToken(
        Guid authorId,
        string email = "manuel@portfolio.local",
        string issuer = "PortfolioWeb",
        string audience = "PortfolioWebClient",
        string signingKey = "ThisIsATestSigningKeyWithEnoughLength123!",
        DateTime? expiresUtc = null)
    {
        var now = DateTime.UtcNow;
        var expiration = expiresUtc ?? now.AddMinutes(60);
        var notBefore = expiration <= now
            ? expiration.AddMinutes(-10)
            : now;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new("authorId", authorId.ToString()),
            new(ClaimTypes.Role, "User")
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: notBefore,
            expires: expiration,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string CreateAccessTokenWithoutAuthorId(string email)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Role, "User")
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ThisIsATestSigningKeyWithEnoughLength123!")),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "PortfolioWeb",
            audience: "PortfolioWebClient",
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed class ThrowingAuthorService(Exception exception) : IAuthorService
    {
        public Task<AuthorDto> GetById(Guid id, CancellationToken cancellationToken = default) =>
            throw exception;

        public Task<List<AuthorDto>> GetAll(CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<AuthorDto> Update(PersistAuthorDto authorDto, Guid currentAuthorId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task Delete(Guid id, Guid currentAuthorId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private sealed class ThrowingProjectService(Exception exception) : IProjectService
    {
        public Task<ProjectDto> GetById(Guid id, CancellationToken cancellationToken = default) =>
            throw exception;

        public Task<List<ProjectDto>> GetAll(CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<ProjectDto> Create(CreateProjectDto projectDto, Guid currentAuthorId, CancellationToken cancellationToken = default) =>
            throw exception;

        public Task<ProjectDto> Update(Guid id, UpdateProjectDto projectDto, Guid currentAuthorId, CancellationToken cancellationToken = default) =>
            throw exception;

        public Task Delete(Guid id, Guid currentAuthorId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private sealed class ThrowingAuthService(Exception exception) : IAuthService
    {
        public Task<AuthResponseDto> Register(RegisterUserDto registerUserDto, CancellationToken cancellationToken = default) =>
            throw exception;

        public Task<AuthResponseDto> Login(LoginUserDto loginUserDto, CancellationToken cancellationToken = default) =>
            throw exception;

        public Task EnsureCurrentUserIsActive(string email, CancellationToken cancellationToken = default) =>
            throw exception;
    }

    private sealed class InMemoryUserRepository : IUserRepository
    {
        private readonly Dictionary<string, User> _usersByEmail = new(StringComparer.Ordinal);

        public User? CreatedUser { get; private set; }

        public Task<User?> GetByEmail(string email, CancellationToken cancellationToken = default)
        {
            _usersByEmail.TryGetValue(email, out var user);

            return Task.FromResult(user);
        }

        public Task<User> Create(User user, CancellationToken cancellationToken = default)
        {
            if (user.Id == Guid.Empty)
            {
                user.Id = Guid.NewGuid();
            }

            if (user.Author.Id == Guid.Empty)
            {
                user.Author.Id = Guid.NewGuid();
            }

            user.Author.UserId = user.Id;
            user.Author.User = user;

            _usersByEmail[user.Email] = user;
            CreatedUser = user;

            return Task.FromResult(user);
        }

        public void AddUser(User user)
        {
            _usersByEmail[user.Email] = user;
            CreatedUser = user;
        }
    }

    private sealed class RaceDuplicateUserRepository(string email, string authorName) : IUserRepository
    {
        private readonly User _conflictingUser = CreatePersistedUser(email, authorName);
        private int _getByEmailCallCount;

        public Task<User?> GetByEmail(string requestedEmail, CancellationToken cancellationToken = default)
        {
            _getByEmailCallCount++;

            return _getByEmailCallCount == 1 ? Task.FromResult<User?>(null) : Task.FromResult(requestedEmail == email ? _conflictingUser : null);
        }

        public Task<User> Create(User user, CancellationToken cancellationToken = default) =>
            throw new InfrastructurePersistenceException("duplicate email");
    }

    private sealed class RealRaceDuplicateUserRepository(
        UserRepository innerRepository,
        DuplicateEmailRaceCoordinator coordinator) : IUserRepository
    {
        public Task<User?> GetByEmail(string email, CancellationToken cancellationToken = default) =>
            innerRepository.GetByEmail(email, cancellationToken);

        public async Task<User> Create(User user, CancellationToken cancellationToken = default)
        {
            if (!string.Equals(user.Email, coordinator.Email, StringComparison.Ordinal))
            {
                return await innerRepository.Create(user, cancellationToken);
            }

            var createCallOrder = Interlocked.Increment(ref coordinator.CreateCallCount);

            if (createCallOrder == 1)
            {
                coordinator.FirstCreateEntered.Release();
                await coordinator.SecondCreateCompleted.WaitAsync(cancellationToken);
                return await innerRepository.Create(user, cancellationToken);
            }

            await coordinator.FirstCreateEntered.WaitAsync(cancellationToken);

            try
            {
                return await innerRepository.Create(user, cancellationToken);
            }
            finally
            {
                coordinator.SecondCreateCompleted.Release();
            }
        }
    }

    private sealed class DuplicateEmailRaceCoordinator(string email)
    {
        public int CreateCallCount;

        public string Email { get; } = email;

        public SemaphoreSlim FirstCreateEntered { get; } = new(0, 1);

        public SemaphoreSlim SecondCreateCompleted { get; } = new(0, 1);
    }

    private sealed class OwnershipProbeAuthorService : IAuthorService
    {
        public Task<AuthorDto> GetById(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<List<AuthorDto>> GetAll(CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<AuthorDto> Update(PersistAuthorDto authorDto, Guid currentAuthorId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AuthorDto
            {
                Id = currentAuthorId,
                Name = authorDto.Name
            });
        }

        public Task Delete(Guid id, Guid currentAuthorId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private class ProblemDetailsResponse
    {
        public int? Status { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Detail { get; set; } = string.Empty;

        public string Instance { get; set; } = string.Empty;
    }

    private sealed class ValidationProblemDetailsResponse : ProblemDetailsResponse
    {
        public Dictionary<string, string[]> Errors { get; set; } = [];
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

    private static IEnumerable<TestCaseData> AuthExceptionMappings()
    {
        yield return new TestCaseData(
            new InvalidAuthRequestException("invalid auth request"),
            HttpStatusCode.BadRequest,
            "Invalid auth request");
        yield return new TestCaseData(
            new DuplicateEmailException("manuel@portfolio.local"),
            HttpStatusCode.Conflict,
            "Duplicate email");
        yield return new TestCaseData(
            new InvalidCredentialsException(),
            HttpStatusCode.Unauthorized,
            "Invalid credentials");
        yield return new TestCaseData(
            new InactiveUserException(),
            HttpStatusCode.Forbidden,
            "Inactive user");
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

    private static User CreatePersistedUser(
        string email,
        string authorName,
        bool isActive = true,
        string? passwordHash = null)
    {
        var userId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var user = new User(
            email,
            passwordHash ?? HashForTests("password"),
            new DateTimeOffset(2026, 06, 24, 0, 0, 0, TimeSpan.Zero),
            UserRole.User,
            isActive)
        {
            Id = userId
        };

        var author = new Author(authorName)
        {
            Id = authorId,
            UserId = userId,
            User = user
        };

        user.Author = author;

        return user;
    }

    private static string HashForTests(string password)
    {
        var salt = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(16));
        var hash = Convert.ToBase64String(System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
            password,
            Convert.FromBase64String(salt),
            100_000,
            System.Security.Cryptography.HashAlgorithmName.SHA256,
            32));

        return $"100000.{salt}.{hash}";
    }

    private async Task ResetApiTestDatabaseAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PortfolioWebDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();
    }

    private async Task EnsurePostgreSqlAvailableOrIgnoreAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PortfolioWebDbContext>();
        var connectionString = dbContext.Database.GetConnectionString()
            ?? throw new InvalidOperationException("PostgreSQL connection string is not configured for API integration tests.");
        var builder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Database = "postgres"
        };

        try
        {
            await using var connection = new NpgsqlConnection(builder.ConnectionString);
            await connection.OpenAsync();
        }
        catch
        {
            Assert.Ignore("PostgreSQL is not available for API integration tests.");
        }
    }
}

