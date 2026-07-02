using Microsoft.EntityFrameworkCore;
using Npgsql;
using PortfolioWeb.Infrastructure.Persistence;

namespace PortfolioWeb.Infrastructure.Tests.Helpers;

internal static class PostgreSqlDbContextFactory
{
    private const string Host = "localhost";
    private const int Port = 5432;
    private const string Username = "postgres";
    private const string Password = "postgres";

    public static PortfolioWebDbContext Create(string databaseName)
    {
        var options = new DbContextOptionsBuilder<PortfolioWebDbContext>()
            .UseNpgsql(BuildConnectionString(databaseName))
            .Options;

        return new PortfolioWebDbContext(options);
    }

    public static async Task ResetDatabaseAsync(string databaseName, CancellationToken cancellationToken = default)
    {
        await using var context = Create(databaseName);

        await context.Database.EnsureDeletedAsync(cancellationToken);
        await context.Database.MigrateAsync(cancellationToken);
    }

    public static async Task<bool> IsServerAvailableAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(BuildConnectionString("postgres"));

        try
        {
            await connection.OpenAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string BuildConnectionString(string databaseName)
    {
        return $"Host={Host};Port={Port};Database={databaseName};Username={Username};Password={Password}";
    }
}
