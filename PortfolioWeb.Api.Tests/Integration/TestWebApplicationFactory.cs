using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PortfolioWeb.Api.Tests.Integration;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Issuer"] = "PortfolioWeb",
                ["Authentication:Audience"] = "PortfolioWebClient",
                ["Authentication:SigningKey"] = "ThisIsATestSigningKeyWithEnoughLength123!",
                ["Authentication:ExpirationMinutes"] = "60"
            });
        });
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });
    }
}
