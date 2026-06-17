using PortfolioWeb.Core.Contracts.Exceptions;

namespace PortfolioWeb.Core.Contracts.Tests.Exceptions;

public class InfrastructureDerivedExceptionsTest
{
    [TestCase(typeof(InfrastructureConnectionException))]
    [TestCase(typeof(InfrastructureQueryException))]
    [TestCase(typeof(InfrastructurePersistenceException))]
    public void DerivedInfrastructureExceptions_ShouldInitializeDefaultConstructor(Type exceptionType)
    {
        var exception = (InfrastructureException)Activator.CreateInstance(exceptionType)!;

        Assert.Multiple(() =>
        {
            Assert.That(exception, Is.TypeOf(exceptionType));
            Assert.That(exception.Message, Is.Not.Null.And.Not.Empty);
            Assert.That(exception.InnerException, Is.Null);
        });
    }

    [TestCase(typeof(InfrastructureConnectionException))]
    [TestCase(typeof(InfrastructureQueryException))]
    [TestCase(typeof(InfrastructurePersistenceException))]
    public void DerivedInfrastructureExceptions_ShouldInitializeMessageConstructor(Type exceptionType)
    {
        var exception = (InfrastructureException)Activator.CreateInstance(exceptionType, "boom")!;

        Assert.Multiple(() =>
        {
            Assert.That(exception, Is.TypeOf(exceptionType));
            Assert.That(exception.Message, Is.EqualTo("boom"));
            Assert.That(exception.InnerException, Is.Null);
        });
    }

    [TestCase(typeof(InfrastructureConnectionException))]
    [TestCase(typeof(InfrastructureQueryException))]
    [TestCase(typeof(InfrastructurePersistenceException))]
    public void DerivedInfrastructureExceptions_ShouldInitializeMessageAndInnerExceptionConstructor(Type exceptionType)
    {
        var innerException = new InvalidOperationException("inner");
        var exception = (InfrastructureException)Activator.CreateInstance(exceptionType, "boom", innerException)!;

        Assert.Multiple(() =>
        {
            Assert.That(exception, Is.TypeOf(exceptionType));
            Assert.That(exception.Message, Is.EqualTo("boom"));
            Assert.That(exception.InnerException, Is.SameAs(innerException));
        });
    }
}
