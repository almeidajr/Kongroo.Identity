using Kongroo.BuildingBlocks.Application;
using Kongroo.BuildingBlocks.Domain;
using Kongroo.BuildingBlocks.Infrastructure;
using Kongroo.Identity.Domain;
using Kongroo.Identity.Infrastructure;
using Kongroo.Identity.IntegrationTests.Fixtures;
using Kongroo.Identity.IntegrationTests.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;

namespace Kongroo.Identity.IntegrationTests.BuildingBlocks.Infrastructure;

public sealed class UnitOfWorkTests(PostgreSqlFixture postgreSqlFixture)
    : IClassFixture<PostgreSqlFixture>,
        IAsyncLifetime
{
    private readonly IdentityTestDatabase _database = new(postgreSqlFixture);

    [Fact]
    public async Task CommitAsync_ShouldDispatchEventsAndPersist()
    {
        // Arrange
        await using var context = _database.CreateDbContext();
        var user = User.Create(
            Username.From("kongroo"),
            Email.From("kongroo@example.com"),
            PasswordHash.From("hashed-password"),
            PersonName.From("Kongroo Cloud Games")
        );
        context.Users.Add(user);

        var dispatcher = Substitute.For<IDomainEventDispatcher>();
        var unitOfWork = new UnitOfWork<IdentityDbContext>(context, dispatcher);

        // Act
        await unitOfWork.CommitAsync(TestContext.Current.CancellationToken);

        // Assert
        await dispatcher
            .Received(1)
            .DispatchAsync(
                Arg.Is<IEnumerable<IHasDomainEvents>>(sources => ReferenceEquals(sources.Single(), user)),
                Arg.Any<CancellationToken>()
            );

        context.ChangeTracker.Clear();
        (await context.Users.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(1);
    }

    public async ValueTask InitializeAsync() => await _database.ResetAsync(TestContext.Current.CancellationToken);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
