using PortfolioWeb.Core.Contracts.Exceptions;

namespace PortfolioWeb.Core.Contracts.Tests.Exceptions;

public class InfrastructureExceptionTest
{
    [Test]
    public void InfrastructureException_ShouldInitializeDefaultConstructor()
    {
        var exception = new TestInfrastructureException();

        Assert.Multiple(() =>
        {
            Assert.That(exception.Message, Is.Not.Null.And.Not.Empty);
            Assert.That(exception.InnerException, Is.Null);
        });
    }

    [Test]
    public void InfrastructureException_ShouldInitializeMessageConstructor()
    {
        var exception = new TestInfrastructureException("boom");

        Assert.Multiple(() =>
        {
            Assert.That(exception.Message, Is.EqualTo("boom"));
            Assert.That(exception.InnerException, Is.Null);
        });
    }

    [Test]
    public void InfrastructureException_ShouldInitializeMessageAndInnerExceptionConstructor()
    {
        var innerException = new InvalidOperationException("inner");
        var exception = new TestInfrastructureException("boom", innerException);

        Assert.Multiple(() =>
        {
            Assert.That(exception.Message, Is.EqualTo("boom"));
            Assert.That(exception.InnerException, Is.SameAs(innerException));
        });
    }

    private sealed class TestInfrastructureException : InfrastructureException
    {
        public TestInfrastructureException()
        {
        }

        public TestInfrastructureException(string message) : base(message)
        {
        }

        public TestInfrastructureException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
