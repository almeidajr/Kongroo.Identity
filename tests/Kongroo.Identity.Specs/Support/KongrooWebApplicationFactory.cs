using Microsoft.AspNetCore.Mvc.Testing;

namespace Kongroo.Identity.Specs.Support;

public sealed class KongrooWebApplicationFactory(string databaseConnectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration(
            (_, configurationBuilder) =>
            {
                var testConfiguration = new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Database"] = databaseConnectionString,
                    ["Jwt:Issuer"] = "Kongroo.Identity.Specs",
                    ["Jwt:Audience"] = "Kongroo.Identity.Specs",
                    ["Jwt:SigningKey"] = "Kongroo.Identity.Specs.SigningKey.For.Bdd.Tests",
                    ["Jwt:AccessTokenLifetimeMinutes"] = "15",
                    ["BootstrapAdmin:Username"] = "spec-admin",
                    ["BootstrapAdmin:Email"] = "spec-admin@kongroo.dev",
                    ["BootstrapAdmin:Password"] = "Sup3rSecure!",
                    ["BootstrapAdmin:Name"] = "Specs Admin",
                    ["OutboxProcessing:PollingInterval"] = "00:10:00",
                    ["OutboxProcessing:BatchSize"] = "1",
                };

                configurationBuilder.AddInMemoryCollection(testConfiguration);
            }
        );

        builder.ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders());
    }
}
