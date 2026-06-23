using System.Collections.Concurrent;
using Kongroo.BuildingBlocks.Application;
using Kongroo.BuildingBlocks.Contracts;
using Kongroo.BuildingBlocks.Infrastructure;
using Kongroo.Identity.Application;
using Kongroo.Identity.Domain;
using Kongroo.Identity.Infrastructure;
using Kongroo.Identity.IntegrationTests.Fixtures;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;

namespace Kongroo.Identity.IntegrationTests.Identity.Infrastructure;

public sealed class BusOutboxLifecycleTests(PostgreSqlFixture postgreSqlFixture)
    : IClassFixture<PostgreSqlFixture>,
        IAsyncLifetime
{
    private readonly IdentityTestDatabase _database = new(postgreSqlFixture);
    private static readonly ConcurrentBag<UserCreatedIntegrationEvent> Received = [];

    private IHost BuildHost()
    {
        Received.Clear();
        return new HostBuilder()
            .ConfigureServices(services =>
            {
                services.AddDbContext<IdentityDbContext>(options =>
                    options
                        .UseNpgsql(
                            postgreSqlFixture.ConnectionString,
                            postgres => postgres.MigrationsHistoryTable("migrations", IdentityDbContext.Schema)
                        )
                        .UseSnakeCaseNamingConvention()
                );
                services.AddScoped<IUnitOfWork, UnitOfWork<IdentityDbContext>>();
                services.AddScoped<IDomainEventHandler, UserCreatedDomainEventHandler>();
                services.AddScoped<IDomainEventHandler, UserRoleChangedDomainEventHandler>();
                services.AddMassTransit(bus =>
                {
                    bus.AddEntityFrameworkOutbox<IdentityDbContext>(outbox =>
                    {
                        outbox.UsePostgres();
                        outbox.UseBusOutbox();
                        outbox.QueryDelay = TimeSpan.FromMilliseconds(250);
                    });
                    bus.AddConsumer<CapturingConsumer>();
                    bus.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
                });
            })
            .Build();
    }

    [Fact]
    public async Task Commit_ShouldPersistUserAndOutboxRowAtomically_ThenDeliver()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var host = BuildHost();
        await host.StartAsync(cancellationToken);

        try
        {
            UserId createdUserId;
            await using (var scope = host.Services.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var user = User.Create(
                    Username.From("kongroo"),
                    Email.From("kongroo@example.com"),
                    PasswordHash.From("hashed-password"),
                    PersonName.From("Kongroo Cloud Games")
                );
                createdUserId = user.Id;
                context.Users.Add(user);

                await unitOfWork.CommitAsync(cancellationToken);

                // Atomic: user row AND a staged outbox_message row both present right after commit.
                (await context.Users.CountAsync(cancellationToken)).ShouldBe(1);
                var outboxCount = await context
                    .Database.SqlQuery<long>(
                        $"""SELECT COUNT(*)::bigint AS "Value" FROM "identity"."outbox_message" """
                    )
                    .SingleAsync(cancellationToken);
                outboxCount.ShouldBe(1);
            }

            // Eventual delivery after commit, with mapped fields.
            await WaitUntilAsync(() => !Received.IsEmpty, TimeSpan.FromSeconds(10), cancellationToken);
            var delivered = Received.ShouldHaveSingleItem();
            delivered.UserId.ShouldBe(createdUserId.Value);
            delivered.Email.ShouldBe("kongroo@example.com");
        }
        finally
        {
            await host.StopAsync(cancellationToken);
        }
    }

    [Fact]
    public async Task Commit_WhenSaveFails_ShouldNotPersistAnythingNorDeliver()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var host = BuildHost();
        await host.StartAsync(cancellationToken);

        try
        {
            // Seed an existing user so a second user with the same username violates the unique index.
            await using (var seedScope = host.Services.CreateAsyncScope())
            {
                var context = seedScope.ServiceProvider.GetRequiredService<IdentityDbContext>();
                var unitOfWork = seedScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                context.Users.Add(
                    User.Create(
                        Username.From("dupuser"),
                        Email.From("first@example.com"),
                        PasswordHash.From("hashed-password"),
                        PersonName.From("First User")
                    )
                );
                await unitOfWork.CommitAsync(cancellationToken);
            }

            await WaitUntilAsync(() => !Received.IsEmpty, TimeSpan.FromSeconds(10), cancellationToken);
            Received.Clear();

            // Force a SaveChanges failure: duplicate username.
            await using (var scope = host.Services.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                context.Users.Add(
                    User.Create(
                        Username.From("dupuser"),
                        Email.From("second@example.com"),
                        PasswordHash.From("hashed-password"),
                        PersonName.From("Second User")
                    )
                );

                await Should.ThrowAsync<DbUpdateException>(() => unitOfWork.CommitAsync(cancellationToken));
            }

            // Nothing extra committed, nothing delivered for the failed commit.
            await using (var verifyScope = host.Services.CreateAsyncScope())
            {
                var context = verifyScope.ServiceProvider.GetRequiredService<IdentityDbContext>();
                (await context.Users.CountAsync(cancellationToken)).ShouldBe(1);
                var outboxCount = await context
                    .Database.SqlQuery<long>(
                        $"""SELECT COUNT(*)::bigint AS "Value" FROM "identity"."outbox_message" """
                    )
                    .SingleAsync(cancellationToken);
                outboxCount.ShouldBe(0);
            }

            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            Received.ShouldBeEmpty();
        }
        finally
        {
            await host.StopAsync(cancellationToken);
        }
    }

    public async ValueTask InitializeAsync() => await _database.ResetAsync(TestContext.Current.CancellationToken);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static async Task WaitUntilAsync(
        Func<bool> condition,
        TimeSpan timeout,
        CancellationToken cancellationToken
    )
    {
        var deadline = DateTimeOffset.UtcNow + timeout;
        while (!condition() && DateTimeOffset.UtcNow < deadline)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
        }
    }

    private sealed class CapturingConsumer : IConsumer<UserCreatedIntegrationEvent>
    {
        public Task Consume(ConsumeContext<UserCreatedIntegrationEvent> context)
        {
            Received.Add(context.Message);
            return Task.CompletedTask;
        }
    }
}
