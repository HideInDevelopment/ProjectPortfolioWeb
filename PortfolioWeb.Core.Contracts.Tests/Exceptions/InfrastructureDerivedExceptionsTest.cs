using PortfolioWeb.Core.Contracts.Exceptions;

namespace PortfolioWeb.Core.Contracts.Tests.Exceptions;

public class InfrastructureDerivedExceptionsTest
{
    [TestCaseSource(nameof(InfrastructureExceptionTypes))]
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

    [TestCaseSource(nameof(InfrastructureExceptionTypes))]
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

    [TestCaseSource(nameof(InfrastructureExceptionTypes))]
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

    private static IEnumerable<TestCaseData> InfrastructureExceptionTypes()
    {
        yield return new TestCaseData(typeof(InfrastructureConnectionException));
        yield return new TestCaseData(typeof(InfrastructureQueryException));
        yield return new TestCaseData(typeof(InfrastructurePersistenceException));
    }
}
