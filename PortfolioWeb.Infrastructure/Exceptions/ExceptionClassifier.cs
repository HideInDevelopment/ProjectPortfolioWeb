using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data.Common;

namespace PortfolioWeb.Infrastructure.Exceptions;

public static class ExceptionClassifier
{
    public static bool IsConnectionException(Exception exception)
    {
        return exception is TimeoutException or NpgsqlException { IsTransient: true };
    }

    public static bool IsQueryException(Exception exception)
    {
        return exception is DbException;
    }

    public static bool IsPersistenceException(Exception exception)
    {
        return exception is DbUpdateException or DbUpdateConcurrencyException;
    }
}
