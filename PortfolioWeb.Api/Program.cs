using Microsoft.EntityFrameworkCore;
using PortfolioWeb.Application;
using PortfolioWeb.Api.ExceptionHandling;
using PortfolioWeb.Infrastructure;
using PortfolioWeb.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
// TODO: Review and update EF Core packages when a newer patched version is adopted to remove this temporary transitive package pin.

var app = builder.Build();

app.UseExceptionHandler();

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<PortfolioWebDbContext>();
    var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

    if (pendingMigrations.Any())
    {
        await dbContext.Database.MigrateAsync();
    }
}

app.MapOpenApi();
app.MapScalarApiReference("/scalar");
app.MapControllers();

app.Run();

public partial class Program;
