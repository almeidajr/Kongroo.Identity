using Kongroo.BuildingBlocks.Infrastructure;
using Kongroo.Identity.Infrastructure;
using Kongroo.Identity.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Kongroo.Identity.IntegrationTests.Identity;

public sealed class IdentityTestDatabase(PostgreSqlFixture fixture)
{
    public IdentityDbContext CreateDbContext() =>
        new(
            new DbContextOptionsBuilder<IdentityDbContext>()
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
                .AddInterceptors(new OutboxMessagesInterceptor())
                .UseNpgsql(
                    fixture.ConnectionString,
                    postgresOptions => postgresOptions.MigrationsHistoryTable("migrations", IdentityDbContext.Schema)
                )
                .UseSnakeCaseNamingConvention()
                .Options
        );

    public async Task ResetAsync(CancellationToken cancellationToken)
    {
        await using var context = CreateDbContext();
        await context.Database.MigrateAsync(cancellationToken);
        var truncateTablesSql = $"""
            TRUNCATE TABLE
                "{IdentityDbContext.Schema}"."outbox_messages",
                "{IdentityDbContext.Schema}"."users"
            CASCADE;
            """;
        await context.Database.ExecuteSqlRawAsync(truncateTablesSql, cancellationToken);
    }
}
