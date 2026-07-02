using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using PortfolioWeb.Application;
using PortfolioWeb.Api.ExceptionHandling;
using PortfolioWeb.Infrastructure;
using PortfolioWeb.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problemDetails = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed",
            Detail = "One or more validation errors occurred.",
            Instance = context.HttpContext.Request.Path,
            Extensions =
            {
                ["traceId"] = context.HttpContext.TraceIdentifier
            }
        };

        return new BadRequestObjectResult(problemDetails)
        {
            ContentTypes =
            {
                "application/problem+json"
            }
        };
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Description = "Bearer {token}"
        };

        return Task.CompletedTask;
    });

    options.AddOperationTransformer((operation, context, _) =>
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        var isAnonymous = metadata.OfType<IAllowAnonymous>().Any();
        var requiresAuthorization = metadata.OfType<IAuthorizeData>().Any();

        if (isAnonymous || !requiresAuthorization)
        {
            return Task.CompletedTask;
        }

        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
        });

        return Task.CompletedTask;
    });
});
builder.Services.AddProblemDetails();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var issuer = GetRequiredConfiguration(builder.Configuration, "Authentication:Issuer");
        var audience = GetRequiredConfiguration(builder.Configuration, "Authentication:Audience");
        var signingKey = GetRequiredConfiguration(builder.Configuration, "Authentication:SigningKey");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
// TODO: Review and update EF Core packages when a newer patched version is adopted to remove this temporary transitive package pin.

var app = builder.Build();
ValidateSecurityConfiguration(app.Configuration);

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

if (!app.Environment.IsEnvironment("Testing"))
{
    var applyMigrationsOnStartup = app.Configuration.GetValue("Database:ApplyMigrationsOnStartup", app.Environment.IsDevelopment());

    if (applyMigrationsOnStartup)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PortfolioWebDbContext>();
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

        if (pendingMigrations.Any())
        {
            await dbContext.Database.MigrateAsync();
        }
    }
}

if (app.Environment.IsDevelopment() ||
    app.Environment.IsEnvironment("Testing") ||
    app.Configuration.GetValue<bool>("Features:ExposeApiDocs"))
{
    app.MapOpenApi();
    app.MapScalarApiReference("/scalar");
}

app.MapControllers();

app.Run();

static string GetRequiredConfiguration(IConfiguration configuration, string key)
{
    var value = configuration[key];

    if (string.IsNullOrWhiteSpace(value))
    {
        throw new InvalidOperationException($"{key} is not configured.");
    }

    return value;
}

static void ValidateSecurityConfiguration(IConfiguration configuration)
{
    _ = GetRequiredConfiguration(configuration, "ConnectionStrings:PortfolioWebDatabase");
    _ = GetRequiredConfiguration(configuration, "Authentication:Issuer");
    _ = GetRequiredConfiguration(configuration, "Authentication:Audience");
    var signingKey = GetRequiredConfiguration(configuration, "Authentication:SigningKey");
    var authenticationExpirationMinutes = GetRequiredConfiguration(configuration, "Authentication:ExpirationMinutes");

    if (signingKey.Length < 32)
    {
        throw new InvalidOperationException("Authentication signing key must be at least 32 characters long.");
    }

    if (!int.TryParse(authenticationExpirationMinutes, out var parsedAuthenticationExpirationMinutes) ||
        parsedAuthenticationExpirationMinutes <= 0)
    {
        throw new InvalidOperationException("Authentication expiration must be a positive integer.");
    }
}
