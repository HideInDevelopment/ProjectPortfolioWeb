using Microsoft.EntityFrameworkCore;
using Npgsql;
using PortfolioWeb.Core.Contracts.Exceptions;
using System.Data.Common;

namespace PortfolioWeb.Core.Contracts.Tests.Exceptions;

public class ExceptionClassifierTest
{
    [Test]
    public void IsConnectionException_ShouldReturnTrue_WhenExceptionIsTimeoutException()
    {
        var result = ExceptionClassifier.IsConnectionException(new TimeoutException());

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsConnectionException_ShouldReturnTrue_WhenExceptionIsTransientNpgsqlException()
    {
        var result = ExceptionClassifier.IsConnectionException(new FakeTransientNpgsqlException());

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsConnectionException_ShouldReturnFalse_WhenExceptionIsNotAConnectionException()
    {
        var result = ExceptionClassifier.IsConnectionException(new Exception("boom"));

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsQueryException_ShouldReturnTrue_WhenExceptionIsDbException()
    {
        var result = ExceptionClassifier.IsQueryException(new FakeDbException("boom"));

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsQueryException_ShouldReturnFalse_WhenExceptionIsNotADbException()
    {
        var result = ExceptionClassifier.IsQueryException(new Exception("boom"));

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsPersistenceException_ShouldReturnTrue_WhenExceptionIsDbUpdateException()
    {
        var result = ExceptionClassifier.IsPersistenceException(new DbUpdateException("boom"));

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsPersistenceException_ShouldReturnTrue_WhenExceptionIsDbUpdateConcurrencyException()
    {
        var result = ExceptionClassifier.IsPersistenceException(new DbUpdateConcurrencyException("boom"));

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsPersistenceException_ShouldReturnFalse_WhenExceptionIsNotAPersistenceException()
    {
        var result = ExceptionClassifier.IsPersistenceException(new Exception("boom"));

        Assert.That(result, Is.False);
    }

    private sealed class FakeDbException(string message) : DbException(message);

    private sealed class FakeTransientNpgsqlException : NpgsqlException
    {
        public FakeTransientNpgsqlException()
            : base("Transient error")
        {
        }

        public override bool IsTransient => true;
    }
}
