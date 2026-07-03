using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PortfolioWeb.Api.Tests.Integration;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    internal const string PostgreSqlConnectionString =
        "Host=localhost;Port=5432;Database=portfolio_web_api_tests;Username=postgres;Password=postgres";
    private const string AuthenticationIssuer = "PortfolioWeb";
    private const string AuthenticationAudience = "PortfolioWebClient";
    private const string AuthenticationSigningKey = "ThisIsATestSigningKeyWithEnoughLength123!";
    private const string AuthenticationExpirationMinutes = "60";

    public TestWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__PortfolioWebDatabase", PostgreSqlConnectionString);
        Environment.SetEnvironmentVariable("Authentication__Issuer", AuthenticationIssuer);
        Environment.SetEnvironmentVariable("Authentication__Audience", AuthenticationAudience);
        Environment.SetEnvironmentVariable("Authentication__SigningKey", AuthenticationSigningKey);
        Environment.SetEnvironmentVariable("Authentication__ExpirationMinutes", AuthenticationExpirationMinutes);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PortfolioWebDatabase"] = PostgreSqlConnectionString,
                ["Authentication:Issuer"] = AuthenticationIssuer,
                ["Authentication:Audience"] = AuthenticationAudience,
                ["Authentication:SigningKey"] = AuthenticationSigningKey,
                ["Authentication:ExpirationMinutes"] = AuthenticationExpirationMinutes
            });
        });
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });
    }
}
