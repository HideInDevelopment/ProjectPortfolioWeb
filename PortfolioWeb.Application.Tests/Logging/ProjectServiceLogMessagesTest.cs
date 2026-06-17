using Microsoft.Extensions.Logging;
using PortfolioWeb.Application.Services;

namespace PortfolioWeb.Application.Tests.Logging;

public class ProjectServiceLogMessagesTest
{
    [Test]
    public void ProjectServiceLogMessages_ShouldLogForEveryDeclaredMethod_WhenLoggingIsEnabled()
    {
        var loggerType = typeof(ProjectService).Assembly.GetType(
            "PortfolioWeb.Application.Logging.ProjectServiceLogMessages",
            throwOnError: true)!;
        var methods = loggerType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        var logger = new TestLogger();

        foreach (var method in methods)
        {
            var arguments = BuildArguments(method, logger);
            method.Invoke(null, arguments);
        }

        Assert.Multiple(() =>
        {
            Assert.That(logger.Entries, Has.Count.EqualTo(methods.Length));
            Assert.That(logger.Entries, Has.All.Not.Empty);
        });
    }

    private static object?[] BuildArguments(System.Reflection.MethodInfo method, ILogger logger)
    {
        return method.GetParameters()
            .Select(parameter => (object?) (parameter.ParameterType switch
            {
                var type when type == typeof(ILogger) => logger,
                var type when type == typeof(Guid) => Guid.NewGuid(),
                var type when type == typeof(string) => "value",
                var type when type == typeof(int) => 1,
                _ => throw new InvalidOperationException($"Unsupported parameter type '{parameter.ParameterType.Name}' in method '{method.Name}'.")
            }))
            .ToArray();
    }

    private sealed class TestLogger : ILogger
    {
        public List<string> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add(formatter(state, exception));
        }
    }
}
