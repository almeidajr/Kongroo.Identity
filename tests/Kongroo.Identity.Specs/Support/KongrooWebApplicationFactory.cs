using System.Globalization;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Kongroo.Identity.Specs.Support;

public sealed class KongrooWebApplicationFactory(
    string databaseConnectionString,
    string rabbitMqHost,
    int rabbitMqPort,
    string rabbitMqUsername,
    string rabbitMqPassword
) : WebApplicationFactory<Program>
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
                    ["RabbitMq:Host"] = rabbitMqHost,
                    ["RabbitMq:Port"] = rabbitMqPort.ToString(CultureInfo.InvariantCulture),
                    ["RabbitMq:User"] = rabbitMqUsername,
                    ["RabbitMq:Pass"] = rabbitMqPassword,
                    ["Jwt:Issuer"] = "Kongroo.Identity.Specs",
                    ["Jwt:Audience"] = "Kongroo.Identity.Specs",
                    ["Jwt:SigningKey"] = "Kongroo.Identity.Specs.SigningKey.For.Bdd.Tests",
                    ["Jwt:AccessTokenLifetimeMinutes"] = "15",
                    ["BootstrapAdmin:Username"] = "spec-admin",
                    ["BootstrapAdmin:Email"] = "spec-admin@kongroo.dev",
                    ["BootstrapAdmin:Password"] = "Sup3rSecure!",
                    ["BootstrapAdmin:Name"] = "Specs Admin",
                };

                configurationBuilder.AddInMemoryCollection(testConfiguration);
            }
        );

        builder.ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders());
    }
}
